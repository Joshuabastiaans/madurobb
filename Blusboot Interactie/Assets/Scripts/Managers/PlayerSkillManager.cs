using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        public bool isPlayerActive = false;
    }

    public List<PlayerData> players = new List<PlayerData>();

    void Start()
    {
        // Initialize players (assuming two players)
        players.Add(new PlayerData { playerId = 1, playerName = "Player 1" });
        players.Add(new PlayerData { playerId = 2, playerName = "Player 2" });
    }

    public void StartWave()
    {
        foreach (var player in players)
        {
            player.waveStartTime = Time.time;
            player.waveEndTime = 0f;
            player.waveTimeTaken = 0f;
            player.totalExtinguishedDamage = 0; // Reset for new wave
        }
    }
    public void Extinguish(float amount, int playerId)
    {
        PlayerData player = players.Find(p => p.playerId == playerId);
        if (player != null)
        {
            player.totalExtinguishedDamage += (int)amount;
        }

    }
    public void FireExtinguished(int playerId)
    {
        PlayerData player = players.Find(p => p.playerId == playerId);
        if (player != null)
        {
            // If it's the first fire they extinguished in this wave, record wave end time
            if (player.waveEndTime == 0f)
            {
                player.waveEndTime = Time.time;
                player.waveTimeTaken = player.waveEndTime - player.waveStartTime;
                player.totalTimeTaken += player.waveTimeTaken;
            }
        }
    }

    public void EndWave()
    {
        foreach (var player in players)
        {
            DetermineSkillLevel(player);
            print(player.playerName + " is a " + player.skillLevel + " firefighter!");
        }
    }

    private void DetermineSkillLevel(PlayerData player)
    {
        // Calculate efficiency score (damage per second)
        float efficiencyScore = player.totalExtinguishedDamage / player.waveTimeTaken;

        print(player.playerName + " took " + player.waveTimeTaken + " seconds for the wave while extinguishing " + player.totalExtinguishedDamage + " damage. Efficiency score: " + efficiencyScore);
        if (player.totalExtinguishedDamage >= 10)
        {
            player.isPlayerActive = true;
        }
        else
        {
            player.isPlayerActive = false;
        }
        // Set thresholds based on efficiency score
        if (efficiencyScore >= 60f) // Adjust thresholds as needed
        {
            player.skillLevel = SkillLevel.Advanced;
        }
        else if (efficiencyScore >= 40f)
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
        List<PlayerData> activePlayers = new List<PlayerData>();
        foreach (var player in players)
        {
            if (player.totalExtinguishedDamage > 0)
            {
                activePlayers.Add(player);
            }
        }
        return activePlayers;
    }
}
