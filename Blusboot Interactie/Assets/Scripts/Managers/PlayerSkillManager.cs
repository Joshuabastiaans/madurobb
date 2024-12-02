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
        public int totalFiresExtinguished = 0;

        // Wave-specific data
        public float waveStartTime = 0f;
        public float waveEndTime = 0f;
        public float waveTimeTaken = 0f;
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
            player.totalFiresExtinguished = 0; // Reset for new wave
        }
    }

    public void FireExtinguished(int playerId)
    {
        PlayerData player = players.Find(p => p.playerId == playerId);
        if (player != null)
        {
            player.totalFiresExtinguished++;

            // If it's the first fire they extinguished in this wave, record wave end time
            if (player.waveEndTime == 0f)
            {
                player.waveEndTime = Time.time;
                player.waveTimeTaken = player.waveEndTime - player.waveStartTime;
                player.totalTimeTaken += player.waveTimeTaken;

                // Determine skill level based on wave time
                DetermineSkillLevel(player);
            }
        }
    }

    private void DetermineSkillLevel(PlayerData player)
    {
        if (player.waveTimeTaken <= 10f) // Adjust thresholds as needed
        {
            player.skillLevel = SkillLevel.Advanced;
        }
        else if (player.waveTimeTaken <= 20f)
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
            if (player.totalFiresExtinguished > 0)
            {
                activePlayers.Add(player);
            }
        }
        return activePlayers;
    }
}
