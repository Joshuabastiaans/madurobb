using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    public AudioManager audioManager;
    public PlayerSkillManager playerSkillManager;
    public TurnOnWater turnOnWaterP1;
    public TurnOnWater turnOnWaterP2;

    [Header("Large and Small Fire Points")]
    public List<FireController> largeFirePoints; // The major/large fires
    public List<FireController> smallFirePoints; // The smaller fires

    [Header("Inactivity Settings")]
    public float inactivityTimeout = 60f;
    private bool isPlayerActive = false;
    private float inactivityTimer = 0f;
    private bool isExperienceActive = true;
    public bool allFiresActiveBegin = false;

    // Crowd emotion tracking
    private AudioManager.Emotion currentEmotion = AudioManager.Emotion.Neutral;

    // Used to ensure we only run the main sequence once
    private bool sequenceStarted = false;

    private void Start()
    {
        // Make sure references are found if not assigned in Inspector
        if (!audioManager) audioManager = FindAnyObjectByType<AudioManager>();
        if (!playerSkillManager) playerSkillManager = FindAnyObjectByType<PlayerSkillManager>();

        if (audioManager == null)
            Debug.LogError("WaveManager: AudioManager not found in the scene.");
        if (playerSkillManager == null)
            Debug.LogError("WaveManager: PlayerSkillManager not found in the scene.");
        if (turnOnWaterP1 == null)
            Debug.LogError("WaveManager: TurnOnWater not found in the scene.");
        if (turnOnWaterP2 == null)
            Debug.LogError("WaveManager: TurnOnWater2 not found in the scene.");
    }

    private void Update()
    {
        // --- Handle inactivity ---
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
        isPlayerActive = false; // reset each frame

        // --- Optional debug keys ---
        if (Input.GetKeyDown(KeyCode.A))
        {
            StopExperience();     // If you want to restart from scratch
            audioManager.SetVolume(0.2f);
            audioManager.SetCrowdEmotion(AudioManager.Emotion.Neutral, 1f);
            StartFireSequence();  // Start the new logic
            audioManager?.StartPreWaveAudio();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            turnOnWaterP1.TurnOn();
            turnOnWaterP2.TurnOn();
            audioManager?.StartWaveAudio();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            StopExperience();     // If you want to restart from scratch
            audioManager.SetVolume(0.2f);
            audioManager.SetCrowdEmotion(AudioManager.Emotion.Neutral, 1f);
            StartFireSequence();  // Start the new logic
            turnOnWaterP1.TurnOn();
            turnOnWaterP2.TurnOn();
            audioManager?.StartWaveAudio();

        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            StopExperience();
        }
    }

    /// <summary>
    /// Call this when a user input happens to reset inactivity timer.
    /// </summary>
    public void RegisterPlayerActivity()
    {
        isPlayerActive = true;
    }

    /// <summary>
    /// The main logic: 
    /// 1) Ignite middle large. 
    /// 2) Wait until extinguished => Start skill tracking. 
    /// 3) Ignite all other large fires. 
    /// 4) Wait until only 1 large remains => End skill tracking. 
    /// 5) Ignite small fires based on new skill levels.
    /// </summary>
    public void StartFireSequence()
    {
        if (!sequenceStarted)
        {
            sequenceStarted = true;
            StartCoroutine(FireSequenceRoutine());
        }
        else
        {
            Debug.LogWarning("Fire sequence already started!");
        }
    }

    private IEnumerator FireSequenceRoutine()
    {
        // --- Step 1: Ignite only 1 large fire in the middle ---
        FireController middleFire = largeFirePoints[largeFirePoints.Count / 2];
        middleFire.StartFire();
        bool middleFireExtinguished = false;

        bool middleFireHalfway = false;
        // Subscribe to the extinguished event for the middle fire
        middleFire.OnFireExtinguished += (fireController, playerId) =>
        {
            middleFireExtinguished = true;
            // Unsubscribe to avoid memory leaks
            middleFire.OnFireExtinguished -= (fireController, playerId2) => { };
        };

        Debug.Log("Middle large fire ignited. Waiting for extinguish...");

        // Check if allFiresActiveBegin is true
        if (!allFiresActiveBegin)
        {
            // Wait until the middle fire is extinguished
            while (!middleFireHalfway)
            {
                if (middleFire.GetCurrentHealth() <= 50f)
                {
                    middleFireHalfway = true;
                }
                yield return null;
            }
        }

        Debug.Log("Middle large fire extinguished!");

        // --- Step 2: Start skill tracking (like a new wave) ---
        playerSkillManager.StartWave();
        Debug.Log("Now tracking skill (StartWave).");

        // --- Step 3: Ignite all other large fires ---
        foreach (var largeFire in largeFirePoints)
        {
            if (largeFire != middleFire)
                largeFire.StartFire();
        }
        Debug.Log("All other large fires ignited.");

        // --- Step 4: Wait until only 1 large fire remains active ---
        while (CountActiveFires(largeFirePoints) > 1)
        {
            yield return null;
        }

        // Once down to 1 active large fire, let's evaluate skill
        Debug.Log("Only 1 large fire remains. Evaluating skill...");
        playerSkillManager.EndWave();

        // --- Step 5: Ignite small fires based on skill ---
        PlayerSkillManager.SkillLevel overallSkill = DetermineOverallSkill();
        IgniteSmallFiresBasedOnSkill(overallSkill);

        // --- Step 6: Wait until all fires are extinguished ---
        while (CountActiveFires(largeFirePoints) > 0 || CountActiveFires(smallFirePoints) > 0)
        {
            yield return null;
        }

        Debug.Log("All fires extinguished.");
        audioManager?.EndWaveAudio(1f);
        StartCoroutine(SetCrowdPositive());
        turnOnWaterP1.TurnOff();
        turnOnWaterP2.TurnOff();
    }

    /// <summary>
    /// Stop all coroutines and reset relevant flags/fires.
    /// </summary>
    public void StopExperience()
    {
        StopAllCoroutines();
        sequenceStarted = false;

        // Turn off water if desired
        turnOnWaterP1.TurnOff();
        turnOnWaterP2.TurnOff();

        // Extinguish all fires
        foreach (var largeFire in largeFirePoints)
        {
            if (largeFire.isFireActive)
                largeFire.StopFireImmediately(); // or however you extinguish forcibly
        }
        foreach (var smallFire in smallFirePoints)
        {
            if (smallFire.isFireActive)
                smallFire.StopFireImmediately();
        }

        Debug.Log("Experience stopped. Fires reset.");
    }

    /// <summary>
    /// Count how many from the list are still active (not extinguished).
    /// </summary>
    private int CountActiveFires(List<FireController> fireList)
    {
        int activeCount = 0;
        foreach (var f in fireList)
        {
            if (f != null && f.isFireActive)
                activeCount++;
        }
        return activeCount;
    }

    /// <summary>
    /// Determine an overall skill from the set of players (e.g. highest skill).
    /// </summary>
    private PlayerSkillManager.SkillLevel DetermineOverallSkill()
    {
        List<PlayerSkillManager.PlayerData> activePlayers = playerSkillManager.GetActivePlayers();
        PlayerSkillManager.SkillLevel highest = PlayerSkillManager.SkillLevel.Beginner;

        foreach (var p in activePlayers)
        {
            if (p.skillLevel > highest)
            {
                highest = p.skillLevel;
            }
        }

        Debug.Log("Overall skill determined to be: " + highest);
        return highest;
    }

    /// <summary>
    /// Ignite smaller fire points based on skill.
    /// </summary>
    private void IgniteSmallFiresBasedOnSkill(PlayerSkillManager.SkillLevel skillLevel)
    {
        // Decide how many small fires to light
        int countToIgnite = 0;
        switch (skillLevel)
        {
            case PlayerSkillManager.SkillLevel.Advanced:
                countToIgnite = smallFirePoints.Count; // 100%
                break;
            case PlayerSkillManager.SkillLevel.Intermediate:
                countToIgnite = smallFirePoints.Count / 2; // 50%
                break;
            case PlayerSkillManager.SkillLevel.Beginner:
            default:
                countToIgnite = 0; // none
                break;
        }

        // Shuffle the small list so we ignite random ones if partial
        ShuffleList(smallFirePoints);

        // Ignite the chosen number of small fires
        for (int i = 0; i < countToIgnite; i++)
        {
            smallFirePoints[i].StartFire();
        }

        Debug.Log($"Ignited {countToIgnite} small fires for skill level {skillLevel}.");
    }

    private IEnumerator SetCrowdPositive()
    {
        audioManager.SetVolume(0.5f);
        audioManager.SetCrowdEmotion(AudioManager.Emotion.Positive, 1f);
        yield return new WaitForSeconds(5);
        audioManager.SetVolume(0.2f);
        audioManager.SetCrowdEmotion(AudioManager.Emotion.Neutral, 1f);
    }

    /// <summary>
    /// Utility to shuffle a list in-place.
    /// </summary>
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
