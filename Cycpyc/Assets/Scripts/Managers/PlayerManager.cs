using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public List<PlayerData> Players { get; private set; } = new List<PlayerData>();

    public List<CardDataSO> CardsInPlayerHand { get; private set; }

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

    public List<CardDataSO> GetPlayerHandCount(PlayerData player)
    {
        return player.SelectedCards;
    }

    public CardDataSO GetPlayerCards(PlayerData player, int index)
    {
        if (index < 0 || index >= player.SelectedCards.Count)
        {
            Debug.LogWarning("Card index out of range.");
            return null;
        }
        return player.SelectedCards[index];
    }

}
