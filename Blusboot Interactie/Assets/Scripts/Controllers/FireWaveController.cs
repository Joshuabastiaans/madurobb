using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWaveController : MonoBehaviour
{
    [Header("Fire Wave Settings")]
    public List<FireController> fireControllers = new List<FireController>();
    public float delayBetweenFires = 1f;
    public float maxSpreadDistance = 5f;

    private AudioManager audioManager;
    WaveManager waveManager;
    TurnOnWater turnOnWater;

    void Start()
    {
        // Get reference to the AudioManager in the scene
        audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("FireWaveController: AudioManager not found in the scene.");
        }
        waveManager = FindFirstObjectByType<WaveManager>();
        turnOnWater = FindFirstObjectByType<TurnOnWater>();
    }

    public void StartFireWave()
    {
        waveManager.StartFireWave();
        turnOnWater.TurnOn();

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
