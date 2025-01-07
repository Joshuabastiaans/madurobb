using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public enum SkillLevel { Beginner, Intermediate, Advanced }

    [System.Serializable]
    public class PlayerData
    {
        public int playerId;
        public string playerName;
        public SkillLevel skillLevel = SkillLevel.Beginner;
        public float totalTimeTaken = 0f;
        public int totalExtinguishedDamage = 0;

        // Wave-specific data
        public float waveStartTime = 0f;
        public float waveEndTime = 0f;
        public float waveTimeTaken = 0f;
        public bool isPlayerActive = true;
    }

    public List<PlayerData> players = new List<PlayerData>();

    void Start()
    {
        // Example initialization for two players
        players.Add(new PlayerData { playerId = 1, playerName = "Player 1" });
        players.Add(new PlayerData { playerId = 2, playerName = "Player 2" });
    }

    public void StartWave()
    {
        // Reset wave-specific timers and extinguished damage
        foreach (var player in players)
        {
            player.waveStartTime = Time.time;
            player.waveEndTime = 0f;
            player.waveTimeTaken = 0f;
            player.totalExtinguishedDamage = 0;
        }
    }

    public void Extinguish(float amount, int playerId)
    {
        // Increase extinguished damage for the relevant player
        PlayerData player = players.Find(p => p.playerId == playerId);
        if (player != null)
        {
            player.totalExtinguishedDamage += (int)amount;
        }
    }

    public void FireExtinguished(int playerId)
    {
        // Currently unused in this simplified flow, but you can add logic if you want
        PlayerData player = players.Find(p => p.playerId == playerId);
        if (player != null)
        {
            // e.g., increment a counter if needed
        }
    }

    public void EndWave()
    {
        // Stop the wave timer and compute skill
        foreach (var player in players)
        {
            if (player.waveEndTime == 0f)
            {
                player.waveEndTime = Time.time;
                player.waveTimeTaken = player.waveEndTime - player.waveStartTime;
                player.totalTimeTaken += player.waveTimeTaken;
            }
            DetermineSkillLevel(player);
            Debug.Log(player.playerName + " is a " + player.skillLevel + " firefighter!");
        }
    }

    private void DetermineSkillLevel(PlayerData player)
    {
        // Example efficiency: “Damage extinguished per second”
        float efficiencyScore = 0f;
        if (player.waveTimeTaken > 0f)
            efficiencyScore = player.totalExtinguishedDamage / player.waveTimeTaken;

        Debug.Log(player.playerName + " took " + player.waveTimeTaken +
                  "s for this wave, extinguished " + player.totalExtinguishedDamage +
                  " damage. Efficiency: " + efficiencyScore);

        // Example threshold checks
        if (player.totalExtinguishedDamage >= 10)
        {
            player.isPlayerActive = true;
        }
        else
        {
            player.isPlayerActive = false;
        }

        // Adjust thresholds to match your difficulty
        if (efficiencyScore >= 100f)
        {
            player.skillLevel = SkillLevel.Advanced;
        }
        else if (efficiencyScore >= 60f)
        {
            player.skillLevel = SkillLevel.Intermediate;
        }
        else
        {
            player.skillLevel = SkillLevel.Beginner;
        }
    }

    public List<PlayerData> GetActivePlayers()
    {
        // Return currently active players (in this example, we return all)
        // Or filter out players who are inactive if desired
        List<PlayerData> activePlayers = new List<PlayerData>();
        foreach (var player in players)
        {
            // If you want to only return those with isPlayerActive == true, do so here
            activePlayers.Add(player);
        }
        return activePlayers;
    }
}
