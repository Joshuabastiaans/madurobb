using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWaveController : MonoBehaviour
{
    [Header("Fire Wave Settings")]
    public List<FireController> fireControllers = new List<FireController>();
    public float delayBetweenFires = 1f;
    public float maxSpreadDistance = 5f;

    [Header("Audio Settings")]
    public AudioClip sirenSoundClip;    // Assign your siren sound effect here
    private AudioSource sirenAudioSource;  // AudioSource to play the siren sound

    void Start()
    {
        // Initialize the AudioSource for the siren sound
        sirenAudioSource = gameObject.AddComponent<AudioSource>();
        sirenAudioSource.clip = sirenSoundClip;
        sirenAudioSource.loop = false;
        sirenAudioSource.playOnAwake = false;
        sirenAudioSource.volume = 1f;  // Adjust volume as needed
    }

    [ContextMenu("Start Fire Wave")]
    public void StartFireWave()
    {
        // Play the siren sound
        if (sirenAudioSource != null && sirenSoundClip != null)
        {
            sirenAudioSource.Play();
        }

        StartCoroutine(FireWaveRoutine());
    }

    private IEnumerator FireWaveRoutine()
    {
        HashSet<FireController> ignitedControllers = new HashSet<FireController>();
        Queue<FireController> fireQueue = new Queue<FireController>();

        if (fireControllers.Count > 0)
        {
            FireController firstFire = fireControllers[0];
            firstFire.StartFire();
            ignitedControllers.Add(firstFire);
            fireQueue.Enqueue(firstFire);

            while (fireQueue.Count > 0)
            {
                FireController currentFire = fireQueue.Dequeue();

                // Wait before spreading to nearby fires
                yield return new WaitForSeconds(delayBetweenFires);

                foreach (FireController fireController in fireControllers)
                {
                    if (!ignitedControllers.Contains(fireController))
                    {
                        float distance = Vector3.Distance(currentFire.transform.position, fireController.transform.position);
                        if (distance <= maxSpreadDistance)
                        {
                            fireController.StartFire();
                            ignitedControllers.Add(fireController);
                            fireQueue.Enqueue(fireController);
                        }
                    }
                }
            }
        }
    }
}
