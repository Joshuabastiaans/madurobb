using System.Collections;
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

    private float fireIntensity;              // Current fire intensity
    private Renderer objectRenderer;          // Renderer to change object color
    private bool isBeingExtinguished = false;
    private bool isBeingExtinguishedDelayed = false;
    public bool isExtinguished = true;        // Fire starts as extinguished
    private bool isFireActive = false;        // Indicates whether the fire is active
    private float extinguishAccumulator = 0f; // Accumulates extinguish amount per frame
    private ParticleSystem fireParticles;
    private ParticleSystem smokeParticles;
    private MaterialPropertyBlock propBlock;

    public delegate void FireExtinguishedHandler(FireController fireController, int playerId);
    public event FireExtinguishedHandler OnFireExtinguished;

    private int lastExtinguishingPlayerId = -1; // -1 indicates no player

    void Start()
    {
        fireIntensity = 0f;
        propBlock = new MaterialPropertyBlock();
        fireParticles = GetComponentInChildren<ParticleSystem>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer == null)
        {
            Debug.LogError("FireController: No Renderer found on the object.");
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

        // Stop and disable fire particles at start
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.enabled = false;
            fireParticles.Stop();
        }

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
    }

    void UpdateFireIntensity()
    {
        if (isBeingExtinguished)
        {
            // Reduce fire intensity based on the accumulated extinguish amount
            fireIntensity -= extinguishAccumulator * extinguishMultiplier * Time.deltaTime;
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
            if (fireParticles != null)
            {
                var emission = fireParticles.emission;
                emission.enabled = false;
                fireParticles.Stop();
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

        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = fireIntensity / 10;
            if (isBeingExtinguishedDelayed)
            {
                emission.rateOverTime = extinguishEmissionMultiplier;
            }
            if (!fireParticles.isPlaying && fireIntensity > 0)
            {
                fireParticles.Play();
                emission.enabled = true;
            }
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
        isFireActive = true;
        isExtinguished = false;
        fireIntensity = maxFireIntensity;

        // Start fire particles
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.enabled = true;
            fireParticles.Play();
        }

        // Update visuals immediately
        UpdateFireVisuals();
    }

    public bool IsExtinguished()
    {
        return isExtinguished;
    }
}
