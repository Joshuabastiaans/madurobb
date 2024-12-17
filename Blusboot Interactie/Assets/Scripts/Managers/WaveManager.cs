using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public List<Wave> waves = new List<Wave>();

    [Header("Fire Points")]
    public List<FireController> allFirePoints; // List of all fire points

    private int currentWaveIndex = 0;
    private AudioManager audioManager;
    private PlayerSkillManager playerSkillManager;

    private bool isPlayerActive = false;
    private float inactivityTimer = 0f;
    public float inactivityTimeout = 60f;
    private bool isExperienceActive = true;
    private bool isWaveActive = false;

    // Progression Tracking
    private List<FireController> activeWaveFires = new List<FireController>(); // Fires in the current wave
    private float maxHealthPerFirePoint = 100f; // Assumed fire point health
    public float waveProgression { get; private set; } = 0f; // Public progression value (0 to 1)
    TurnOnWater turnOnWater;

    private AudioManager.Emotion currentEmotion = AudioManager.Emotion.Neutral; // "None" is a placeholder if not defined, could use null checks or a default state.
    void Start()
    {
        turnOnWater = FindAnyObjectByType<TurnOnWater>();
        audioManager = FindAnyObjectByType<AudioManager>();
        if (audioManager == null)
            Debug.LogError("WaveManager: AudioManager not found in the scene.");

        playerSkillManager = FindAnyObjectByType<PlayerSkillManager>();
        if (playerSkillManager == null)
            Debug.LogError("WaveManager: PlayerSkillManager not found in the scene.");
    }

    public void StartFireWave()
    {
        StartCoroutine(WaveRoutine());
        audioManager.StartWaveAudio();
    }

    private IEnumerator WaveRoutine()
    {
        for (currentWaveIndex = 0; currentWaveIndex < waves.Count; currentWaveIndex++)
        {
            isWaveActive = true;
            Wave wave = waves[currentWaveIndex];
            playerSkillManager.StartWave();
            activeWaveFires.Clear(); // Reset fire tracking

            Dictionary<PlayerSkillManager.PlayerData, List<FireController>> playerFireAssignments = SelectFirePointsForWave(wave);
            yield return StartCoroutine(StartFiresWithSpreadTime(playerFireAssignments));

            // Track active fires for this wave
            foreach (var kvp in playerFireAssignments)
            {
                activeWaveFires.AddRange(kvp.Value);
            }

            // Wait for fires to be extinguished while updating progression
            yield return StartCoroutine(TrackWaveProgression());

            playerSkillManager.EndWave();
            yield return new WaitForSeconds(wave.waveInterval);
        }
        isWaveActive = false;
        audioManager.EndWaveAudio(1f);
        Debug.Log("All waves completed");
    }

    private IEnumerator TrackWaveProgression()
    {
        while (true)
        {
            float totalRemainingHealth = 0f;

            // Calculate remaining health of all fires in the wave
            foreach (FireController fire in activeWaveFires)
            {
                if (fire != null && fire.isFireActive)
                {
                    totalRemainingHealth += fire.GetCurrentHealth();
                }
            }

            // Calculate progression
            float totalPossibleHealth = activeWaveFires.Count * maxHealthPerFirePoint;
            waveProgression = 1f - (totalRemainingHealth / totalPossibleHealth);

            // Debug log progression
            Debug.Log($"Wave Progression: {waveProgression:F2}");

            // Break when all fires are extinguished
            if (totalRemainingHealth <= 0f) break;

            yield return null; // Wait for next frame
        }
    }
    private Dictionary<PlayerSkillManager.PlayerData, List<FireController>> SelectFirePointsForWave(Wave wave)
    {
        Dictionary<PlayerSkillManager.PlayerData, List<FireController>> playerFireAssignments = new Dictionary<PlayerSkillManager.PlayerData, List<FireController>>();
        List<PlayerSkillManager.PlayerData> activePlayers = playerSkillManager.GetActivePlayers();

        // If no active players, do not start any fires
        if (activePlayers.Count == 0)
        {
            Debug.LogWarning("No active players. No fires will be started.");
            // StopExperience();
            return playerFireAssignments;
        }

        int totalFirePoints = wave.maxFireCount;

        // First wave starts in the middle
        if (currentWaveIndex == 0)
        {
            int middleIndex = allFirePoints.Count / 2;
            FireController middleFirePoint = allFirePoints[middleIndex];

            foreach (var player in activePlayers)
            {
                playerFireAssignments[player] = new List<FireController> { middleFirePoint };
                break; // Assign the middle fire point to the first active player
            }
        }
        else
        {
            // Sort active players by skill level (better players first)
            activePlayers.Sort((a, b) => b.skillLevel.CompareTo(a.skillLevel));

            // Assign fires to players based on their skill level
            int remainingFires = totalFirePoints;
            List<FireController> selectedFirePoints = new List<FireController>();

            foreach (var player in activePlayers)
            {
                // Determine the number of fires for this player based on skill level
                int firesToAssign = GetFiresBasedOnSkillLevel(player.skillLevel, remainingFires);

                // Get fire points close to the player
                List<FireController> playerFirePoints = GetFirePointsForPlayer(player, firesToAssign, selectedFirePoints);

                selectedFirePoints.AddRange(playerFirePoints);

                playerFireAssignments[player] = playerFirePoints;

                remainingFires -= playerFirePoints.Count;

                if (remainingFires <= 0)
                    break;
            }
        }

        return playerFireAssignments;
    }

    private int GetFiresBasedOnSkillLevel(PlayerSkillManager.SkillLevel skillLevel, int maxFiresAvailable)
    {
        switch (skillLevel)
        {
            case PlayerSkillManager.SkillLevel.Advanced:
                return Mathf.Min(3, maxFiresAvailable);
            case PlayerSkillManager.SkillLevel.Intermediate:
                return Mathf.Min(2, maxFiresAvailable);
            case PlayerSkillManager.SkillLevel.Beginner:
                return Mathf.Min(1, maxFiresAvailable);
            default:
                return Mathf.Min(1, maxFiresAvailable);
        }
    }

    private List<FireController> GetFirePointsForPlayer(PlayerSkillManager.PlayerData player, int firesToAssign, List<FireController> selectedFirePoints)
    {
        List<FireController> firePoints = new List<FireController>();

        // Get the range of fire points close to the player
        List<FireController> playerFirePoints = GetFirePointsCloseToPlayer(player.playerId);

        // Filter out fire points that are already selected
        playerFirePoints.RemoveAll(fp => selectedFirePoints.Contains(fp));

        // Shuffle the player's fire points to randomize selection
        ShuffleList(playerFirePoints);

        // Select the required number of fire points
        for (int i = 0; i < firesToAssign && i < playerFirePoints.Count; i++)
        {
            firePoints.Add(playerFirePoints[i]);
        }

        Debug.Log("Assigned " + firePoints.Count + " fire points to Player " + player.playerId);

        return firePoints;
    }

    private List<FireController> GetFirePointsCloseToPlayer(int playerId)
    {
        int totalPoints = allFirePoints.Count;
        List<FireController> playerFirePoints = new List<FireController>();

        if (playerId == 1)
        {
            // Player 1 is on the left
            playerFirePoints = allFirePoints.GetRange(0, totalPoints / 2);
        }
        else if (playerId == 2)
        {
            // Player 2 is on the right
            playerFirePoints = allFirePoints.GetRange(totalPoints / 2, totalPoints - (totalPoints / 2));
        }
        else
        {
            // For other players, adjust accordingly
            Debug.LogWarning("Unknown playerId: " + playerId);
        }

        return playerFirePoints;
    }

    private IEnumerator StartFiresWithSpreadTime(Dictionary<PlayerSkillManager.PlayerData, List<FireController>> playerFireAssignments)
    {
        foreach (var kvp in playerFireAssignments)
        {
            print(kvp.Value.Count);
            PlayerSkillManager.PlayerData player = kvp.Key;
            float spreadTime = GetFireSpreadTime(player.skillLevel);

            foreach (FireController fireController in kvp.Value)
            {
                fireController.SetFireSpreadDelay(spreadTime);
                fireController.StartFire();
                fireController.OnFireExtinguished += HandleFireExtinguished;
                yield return new WaitForSeconds(spreadTime);
            }
        }
    }
    private float GetFireSpreadTime(PlayerSkillManager.SkillLevel skillLevel)
    {
        switch (skillLevel)
        {
            case PlayerSkillManager.SkillLevel.Advanced:
                return 0.5f; // Faster spread
            case PlayerSkillManager.SkillLevel.Intermediate:
                return 1.0f;
            case PlayerSkillManager.SkillLevel.Beginner:
                return 1.5f; // Slower spread
            default:
                return 1.0f;
        }
    }

    private IEnumerator WaitForWaveToComplete(Dictionary<PlayerSkillManager.PlayerData, List<FireController>> playerFireAssignments)
    {
        bool waveCompleted = false;
        while (!waveCompleted)
        {
            waveCompleted = true;
            foreach (var kvp in playerFireAssignments)
            {
                foreach (FireController fireController in kvp.Value)
                {
                    if (fireController != null && fireController.isFireActive && !fireController.IsExtinguished())
                    {
                        waveCompleted = false;
                        break;
                    }
                }
                if (!waveCompleted) break;
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
            if (inactivityTimer >= inactivityTimeout && isExperienceActive)
            {
                isExperienceActive = false;
                StopExperience();
            }
        }
        else
        {
            inactivityTimer = 0f;
        }

        isPlayerActive = false; // Reset activity flag each frame

        // if wave progression is .5 or more then make positive souunds
        // check if wave is active

        if (waveProgression < 0.7f && currentEmotion != AudioManager.Emotion.Scared && isWaveActive)
        {
            audioManager.SetCrowdEmotion(AudioManager.Emotion.Scared, 1f);
            currentEmotion = AudioManager.Emotion.Scared;
        }
        else if (waveProgression >= 0.7f && currentEmotion != AudioManager.Emotion.Positive && isWaveActive)
        {
            audioManager.SetCrowdEmotion(AudioManager.Emotion.Positive, 1f);
            currentEmotion = AudioManager.Emotion.Positive;
        }
        if (!isWaveActive && currentEmotion != AudioManager.Emotion.Neutral)
        {
            audioManager.SetCrowdEmotion(AudioManager.Emotion.Neutral, 1f);
            currentEmotion = AudioManager.Emotion.Neutral;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartFireWave();
            turnOnWater.TurnOn();

        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            StopExperience();
        }
    }

    public void StopExperience()
    {
        // Stop all waves and reset
        StopAllCoroutines();
        currentWaveIndex = 0;
        if (audioManager != null && audioManager.inactivityClip != null)
            audioManager.PlayVoiceLine(audioManager.inactivityClip);
        Debug.Log("Experience stopped due to inactivity.");
    }

    public void RegisterPlayerActivity()
    {
        isPlayerActive = true;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}

[System.Serializable]
public class Wave
{
    public string waveName;
    public int maxFireCount;
    public float fireSpreadTime;
    public float waveInterval;
}