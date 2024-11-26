using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<Wave> waves = new List<Wave>();
    public float timeBetweenWaves = 5f;
    public PlayerSkillManager playerSkillManager;

    [Header("Inactivity Settings")]
    public float inactivityTimeout = 30f;

    private float inactivityTimer = 0f;
    private bool isPlayerActive = false;

    void Start()
    {
        // Start the first wave after the game begins
        StartCoroutine(WaveRoutine());
    }

    void Update()
    {
        // Update inactivity timer
        if (!isPlayerActive)
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityTimeout)
            {
                StopExperience();
            }
        }
        else
        {
            inactivityTimer = 0f;
        }

        isPlayerActive = false; // Reset activity flag each frame
    }

    public void RegisterPlayerActivity()
    {
        isPlayerActive = true;
    }

    private IEnumerator WaveRoutine()
    {
        // Wait for player interaction or timeout
        yield return new WaitForSeconds(1f);

        // Start waves
        for (int i = 0; i < waves.Count; i++)
        {
            Wave currentWave = waves[i];
            currentWave.StartWave(playerSkillManager.currentSkillLevel);

            // Wait until all fires in the wave are extinguished
            while (!currentWave.IsWaveCompleted())
            {
                yield return null;
            }

            // Wait before starting the next wave
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        // Experience completed
        playerSkillManager.StopTracking();
    }

    public void StopExperience()
    {
        // Stop all waves and reset
        StopAllCoroutines();
        Debug.Log("Experience stopped due to inactivity.");
    }
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public List<Transform> firePoints;
    public int numberOfFires = 1;

    private List<FireController> activeFires = new List<FireController>();

    public void StartWave(PlayerSkillManager.SkillLevel skillLevel)
    {
        // Adjust the number of fires based on skill level
        switch (skillLevel)
        {
            case PlayerSkillManager.SkillLevel.Beginner:
                numberOfFires = Mathf.Max(1, numberOfFires);
                break;
            case PlayerSkillManager.SkillLevel.Intermediate:
                numberOfFires = Mathf.Max(2, numberOfFires);
                break;
            case PlayerSkillManager.SkillLevel.Advanced:
                numberOfFires = Mathf.Max(3, numberOfFires);
                break;
        }

        // Spawn fires
        for (int i = 0; i < numberOfFires && i < firePoints.Count; i++)
        {
            FireController fire = Object.Instantiate(Resources.Load<FireController>("FirePrefab"), firePoints[i].position, Quaternion.identity);
            fire.StartFire();
            activeFires.Add(fire);
        }
    }

    public bool IsWaveCompleted()
    {
        activeFires.RemoveAll(fire => fire == null); // Clean up destroyed fires
        return activeFires.Count == 0;
    }
}
