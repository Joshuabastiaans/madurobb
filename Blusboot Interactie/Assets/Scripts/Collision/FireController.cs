using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireController : MonoBehaviour
{
    [Header("Fire Settings")]
    public float maxFireIntensity = 100f;     // Maximum fire intensity
    public float rekindleRate = 5f;           // Rate at which the fire rekindles per second

    [Header("Extinguishing Settings")]
    public float extinguishMultiplier = 1f;   // Adjusts how quickly the fire extinguishes
    public float extinguishEmissionMultiplier = 3f;  // Particle emission rate when extinguishing
    public float normalEmissionMultiplier = 1f;    // Normal particle emission rate
    public Gradient fireColorGradient;        // Gradient from extinguished to full fire

    public List<FireController> neighboringFirePoints; // Neighbors for fire spreading
    public float fireSpreadDelay = 1f; // Delay before fire spreads to neighbors

    private float fireIntensity;              // Current fire intensity
    private Renderer objectRenderer;          // Renderer to change object color
    private bool isBeingExtinguished = false;
    private bool isBeingExtinguishedDelayed = false;
    public bool isExtinguished = true;        // Fire starts as extinguished
    public bool isFireActive = false;         // Indicates whether the fire is active
    private float extinguishAccumulator = 0f; // Accumulates extinguish amount per frame
    private MaterialPropertyBlock propBlock;
    private PlayerSkillManager playerSkillManager;
    public delegate void FireExtinguishedHandler(FireController fireController, int playerId);
    public event FireExtinguishedHandler OnFireExtinguished;

    public AudioClip extinguishFireSound;
    public AudioClip extinguishSound;
    public AudioSource extinguishFireAudioSource;
    public AudioSource extinguishAudioSource;

    private int lastExtinguishingPlayerId = -1; // -1 indicates no player
    [Header("Fire Sprite")]
    public GameObject fireSprite;
    private float originalFireSpriteScale;
    public bool isFollower = false;
    private bool isFollowerActivated = false;

    // Class to hold particle system and its original emission rate
    private class ParticleSystemInfo
    {
        public ParticleSystem particleSystem;
        public float originalEmissionRate;

        public ParticleSystemInfo(ParticleSystem ps)
        {
            particleSystem = ps;
            originalEmissionRate = ps.emission.rateOverTime.constant;
        }
    }

    private List<ParticleSystemInfo> particleSystemInfos = new List<ParticleSystemInfo>();

    void Start()
    {
        fireIntensity = 0f;
        propBlock = new MaterialPropertyBlock();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            Debug.LogError("FireController: No Renderer found on the object.");
        }
        if (fireSprite != null)
        {
            fireSprite.SetActive(false);
            originalFireSpriteScale = fireSprite.transform.localScale.x;
        }
        // Ensure a gradient is assigned
        if (fireColorGradient == null)
        {
            // Create a default gradient if none is assigned
            fireColorGradient = new Gradient();
            fireColorGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.gray, 0.0f),
                    new GradientColorKey(Color.red, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
        }

        // Get all particle systems under this object
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = false;
            ps.Stop();

            var psi = new ParticleSystemInfo(ps);
            particleSystemInfos.Add(psi);
        }

        playerSkillManager = FindAnyObjectByType<PlayerSkillManager>();

        // Set the initial color to the extinguished color
        UpdateFireVisuals();
    }

    void Update()
    {
        if (isFireActive)
        {
            if (!isExtinguished)
            {
                UpdateFireIntensity();
            }
            UpdateFireVisuals();
        }
        if (fireIntensity < maxFireIntensity / 2 && !isFollower && !isFollowerActivated && isFireActive)
        {
            isFollowerActivated = true;
            foreach (var fire in neighboringFirePoints)
            {
                if (!fire.isFireActive)
                {
                    fire.StartFire();
                }
            }
        }
    }

    void UpdateFireIntensity()
    {
        if (isBeingExtinguished)
        {
            // Reduce fire intensity based on the accumulated extinguish amount
            float reduction = extinguishAccumulator * extinguishMultiplier * Time.deltaTime;
            reduction = Mathf.Min(reduction, fireIntensity); // Prevent over-reduction
            fireIntensity -= reduction;

            extinguishAccumulator = 0f;     // Reset accumulator after applying
            isBeingExtinguished = false;    // Reset the flag for the next frame
        }
        else
        {
            // Rekindle the fire over time when not being extinguished
            fireIntensity += rekindleRate * Time.deltaTime;
        }

        // Clamp fire intensity between 0 and maxFireIntensity
        fireIntensity = Mathf.Clamp(fireIntensity, 0f, maxFireIntensity);

        if (fireIntensity <= 0)
        {
            isExtinguished = true;
            // Stop fire particles when extinguished
            foreach (var psi in particleSystemInfos)
            {
                var emission = psi.particleSystem.emission;
                emission.enabled = false;
                psi.particleSystem.Stop();
            }

            isFireActive = false;

            // Play the final extinguish sound if not null
            if (extinguishAudioSource != null && extinguishSound != null)
            {
                extinguishAudioSource.PlayOneShot(extinguishSound);
                print("Extinguished sound played");
            }

            // Notify that the fire is extinguished
            if (OnFireExtinguished != null)
            {
                OnFireExtinguished(this, lastExtinguishingPlayerId);
            }
        }
    }

    void UpdateFireVisuals()
    {
        if (objectRenderer != null)
        {
            // Calculate the normalized intensity (0 to 1)
            float normalizedIntensity = fireIntensity / maxFireIntensity;

            // Get the color from the gradient based on normalized intensity
            Color currentColor = fireColorGradient.Evaluate(normalizedIntensity);
            objectRenderer.material.color = currentColor;
            propBlock.SetColor("_Color", currentColor);
            objectRenderer.SetPropertyBlock(propBlock);
        }

        foreach (var psi in particleSystemInfos)
        {
            var emission = psi.particleSystem.emission;

            // Calculate emission rate based on assigned emission value
            float emissionRate = psi.originalEmissionRate * (fireIntensity / maxFireIntensity);

            if (isBeingExtinguishedDelayed)
            {
                emissionRate *= extinguishEmissionMultiplier;
            }

            emission.rateOverTime = emissionRate;

            if (!psi.particleSystem.isPlaying && fireIntensity > 0)
            {
                psi.particleSystem.Play();
                emission.enabled = true;
            }
        }
        UpdateFireSpriteScale();
    }
    private void UpdateFireSpriteScale()
    {
        if (fireSprite == null) return;

        // If there is no intensity, hide the sprite
        if (fireIntensity <= 0)
        {
            fireSprite.SetActive(false);
            return;
        }
        else if (!fireSprite.activeSelf && isFireActive)
        {
            fireSprite.SetActive(true);
        }

        float normalizedIntensity = fireIntensity / maxFireIntensity;

        if (normalizedIntensity > 0.66f)
        {
            // High intensity -> 100%
            fireSprite.transform.localScale = Vector3.one * originalFireSpriteScale;
        }
        else if (normalizedIntensity > 0.33f)
        {
            // Medium intensity -> 75%
            fireSprite.transform.localScale = Vector3.one * 0.75f * originalFireSpriteScale;
        }
        else
        {
            // Low intensity -> 50%
            fireSprite.transform.localScale = Vector3.one * 0.5f * originalFireSpriteScale;
        }
    }

    public void Extinguish(float amount, int playerId)
    {
        if (isExtinguished)
        {
            return;
        }

        isBeingExtinguished = true;
        extinguishAccumulator += amount;
        lastExtinguishingPlayerId = playerId;

        if (extinguishFireAudioSource != null && extinguishFireSound != null && !extinguishFireAudioSource.isPlaying)
        {
            extinguishFireAudioSource.clip = extinguishFireSound;
            extinguishFireAudioSource.Play();
        }

        playerSkillManager?.Extinguish(amount, playerId);

        if (!isBeingExtinguishedDelayed)
        {
            isBeingExtinguishedDelayed = true;
            StopAllCoroutines();
            StartCoroutine(setExtinguishedDelayed());
        }
    }


    IEnumerator setExtinguishedDelayed()
    {
        yield return new WaitForSeconds(3f);
        isBeingExtinguishedDelayed = false;
    }

    public void StartFire()
    {
        if (isFireActive)
            return;

        Debug.Log("Fire is started at " + gameObject.name);
        isFireActive = true;
        isExtinguished = false;
        fireIntensity = maxFireIntensity;
        isFollowerActivated = false;
        // Reset extinguishing state
        extinguishAccumulator = 0f;
        isBeingExtinguished = false;
        isBeingExtinguishedDelayed = false;
        StopAllCoroutines();

        // Start fire particles
        foreach (var psi in particleSystemInfos)
        {
            var emission = psi.particleSystem.emission;
            emission.enabled = true;
            psi.particleSystem.Play();
        }

        // Update visuals immediately
        UpdateFireVisuals();

        // Start spreading coroutine
        HashSet<FireController> visitedFires = new HashSet<FireController>();
        visitedFires.Add(this);
    }

    public void SetFireSpreadDelay(float delay)
    {
        fireSpreadDelay = delay;
    }


    public bool IsExtinguished()
    {
        return isExtinguished;
    }

    public float GetMaxHealth()
    {
        return maxFireIntensity;
    }

    public float GetCurrentHealth()
    {
        return fireIntensity;
    }
}
