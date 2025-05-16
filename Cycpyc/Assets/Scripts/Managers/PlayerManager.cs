using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public List<PlayerData> Players { get; private set; } = new List<PlayerData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TODO: Remove hardcoded players when implementing real player registration
        Players.Add(new PlayerData("Player 1"));
        Players.Add(new PlayerData("Player 2"));
    }

    public void AddPlayer(string playerName)
    {
        Players.Add(new PlayerData(playerName));
    }
}
