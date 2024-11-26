using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public enum SkillLevel { Beginner, Intermediate, Advanced }
    public SkillLevel currentSkillLevel = SkillLevel.Beginner;

    [Header("Skill Thresholds")]
    public float intermediateThreshold = 60f; // Time in seconds to reach intermediate level
    public float advancedThreshold = 30f;     // Time in seconds to reach advanced level

    private float totalTimeTaken = 0f;
    private int totalFiresExtinguished = 0;

    private bool isTracking = false;

    void Start()
    {
        // Start tracking when the game begins
        StartTracking();
    }

    void Update()
    {
        if (isTracking)
        {
            totalTimeTaken += Time.deltaTime;
        }
    }

    public void StartTracking()
    {
        isTracking = true;
        totalTimeTaken = 0f;
        totalFiresExtinguished = 0;
    }

    public void StopTracking()
    {
        isTracking = false;
        DetermineSkillLevel();
    }

    public void FireExtinguished()
    {
        totalFiresExtinguished++;
    }

    private void DetermineSkillLevel()
    {
        if (totalTimeTaken <= advancedThreshold)
        {
            currentSkillLevel = SkillLevel.Advanced;
        }
        else if (totalTimeTaken <= intermediateThreshold)
        {
            currentSkillLevel = SkillLevel.Intermediate;
        }
        else
        {
            currentSkillLevel = SkillLevel.Beginner;
        }
    }
}
