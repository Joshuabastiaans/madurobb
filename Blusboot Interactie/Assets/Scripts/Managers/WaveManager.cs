using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public float timeBetweenWaves = 5f;
    public List<Wave> waves = new List<Wave>();

    [Header("Fire Points")]
    public List<FireController> allFirePoints; // List of all fire points, ordered from left to right

    private int currentWaveIndex = 0;
    private AudioManager audioManager;
    private PlayerSkillManager playerSkillManager;

    private bool isPlayerActive = false; // Indicates whether the player is active
    private float inactivityTimer = 0f; // Timer to track player inactivity
    public float inactivityTimeout = 60f; // Time in seconds before stopping the experience


    void Start()
    {
        // Get references to AudioManager and PlayerSkillManager
        audioManager = FindAnyObjectByType<AudioManager>();
        if (audioManager == null)
            Debug.LogError("WaveManager: AudioManager not found in the scene.");

        playerSkillManager = FindAnyObjectByType<PlayerSkillManager>();
        if (playerSkillManager == null)
            Debug.LogError("WaveManager: PlayerSkillManager not found in the scene.");

        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        // Play wave start sound
        if (audioManager != null && audioManager.waveStartClip != null)
            audioManager.PlayVoiceLine(audioManager.waveStartClip);

        for (currentWaveIndex = 0; currentWaveIndex < waves.Count; currentWaveIndex++)
        {
            Wave wave = waves[currentWaveIndex];
            Debug.Log("Starting Wave: " + wave.waveName);

            // Automatically select fire points for the wave
            List<FireController> selectedFirePoints = SelectFirePointsForWave();

            // Start fires
            foreach (FireController fireController in selectedFirePoints)
            {
                fireController.StartFire();
                fireController.OnFireExtinguished += HandleFireExtinguished;
            }

            // Wait for all fires to be extinguished
            yield return StartCoroutine(WaitForWaveToComplete(selectedFirePoints));

            // Optional: Add delay between waves
            // yield return new WaitForSeconds(5f);
        }

        Debug.Log("All waves completed");
    }

    private List<FireController> SelectFirePointsForWave()
    {
        List<FireController> selectedFirePoints = new List<FireController>();
        List<PlayerSkillManager.PlayerData> activePlayers = playerSkillManager.GetActivePlayers();

        // If no active players, assume both players are active
        if (activePlayers.Count == 0)
        {
            activePlayers.AddRange(playerSkillManager.players);
        }

        foreach (var player in activePlayers)
        {
            // Determine number of fires based on skill level
            int firesToAssign = GetFiresBasedOnSkillLevel(player.skillLevel);

            // Assign fire points to player
            List<FireController> playerFirePoints = GetFirePointsForPlayer(player.playerId, firesToAssign);
            selectedFirePoints.AddRange(playerFirePoints);
        }

        return selectedFirePoints;
    }

    private int GetFiresBasedOnSkillLevel(PlayerSkillManager.SkillLevel skillLevel)
    {
        switch (skillLevel)
        {
            case PlayerSkillManager.SkillLevel.Beginner:
                return 1;
            case PlayerSkillManager.SkillLevel.Intermediate:
                return 2;
            case PlayerSkillManager.SkillLevel.Advanced:
                return 3;
            default:
                return 1;
        }
    }

    private List<FireController> GetFirePointsForPlayer(int playerId, int firesToAssign)
    {
        List<FireController> firePoints = new List<FireController>();

        // Determine the range of fire points for the player
        int startIndex = playerId == 1 ? 0 : allFirePoints.Count / 2;
        int endIndex = playerId == 1 ? allFirePoints.Count / 2 : allFirePoints.Count;

        // Adjust for odd number of fire points
        if (playerId == 2 && allFirePoints.Count % 2 != 0)
            startIndex += 1;

        List<FireController> playerFirePoints = allFirePoints.GetRange(startIndex, endIndex - startIndex);

        // Select the required number of fire points
        for (int i = 0; i < firesToAssign && i < playerFirePoints.Count; i++)
        {
            firePoints.Add(playerFirePoints[i]);
        }

        return firePoints;
    }

    private IEnumerator WaitForWaveToComplete(List<FireController> activeFires)
    {
        bool waveCompleted = false;
        while (!waveCompleted)
        {
            waveCompleted = true;
            foreach (FireController fireController in activeFires)
            {
                if (fireController != null && !fireController.IsExtinguished())
                {
                    waveCompleted = false;
                    break;
                }
            }
            yield return null;
        }
    }

    private void HandleFireExtinguished(FireController fireController, int playerId)
    {
        // Notify PlayerSkillManager
        if (playerSkillManager != null)
        {
            playerSkillManager.FireExtinguished(playerId);
        }

        // Unsubscribe from the event
        fireController.OnFireExtinguished -= HandleFireExtinguished;
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
    public void StopExperience()
    {
        // Stop all waves and reset
        StopAllCoroutines();
        currentWaveIndex = 0;
        audioManager.PlayVoiceLine(audioManager.inactivityClip);
        Debug.Log("Experience stopped due to inactivity.");
    }
    public void RegisterPlayerActivity()
    {
        isPlayerActive = true;
    }

}

[System.Serializable]
public class Wave
{
    public string waveName;
    // Additional wave-specific settings can be added here
}
