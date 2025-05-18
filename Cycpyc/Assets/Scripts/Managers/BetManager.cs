using UnityEngine;
using System.Collections.Generic;
using System;

public class BetManager : MonoBehaviour
{
    public event EventHandler OnStartBetting;

    public static BetManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartBetting()
    {
        ResetBets();

        Debug.Log("Betting started. All previous bets cleared.");
    }

    public void PlaceBet(PlayerData player, BetPunishments playerBet)
    {
        if (!player.PlayerAvailableBets.Contains(playerBet))
        {
            Debug.LogWarning($"{player.PlayerName} tried to place an invalid bet: {playerBet}");
            return;
        }

        player.PlayerBet.Add(playerBet);
        player.PlayerAvailableBets.Remove(playerBet); // remove bet from available
        Debug.Log($"{player.PlayerName} placed a bet of {playerBet}");
    }


    public List<BetPunishments> GetPlayerBets(PlayerData player)
    {
        return new List<BetPunishments>(player.PlayerBet); // return copy 
    }

    public void ConfirmBets()
    {
        Debug.Log("All bets confirmed:");
        foreach (PlayerData player in PlayerManager.Instance.Players)
        {
            if (player.PlayerBet.Count == 0)
            {
                Debug.Log($"{player.PlayerName} has not placed any bets.");
            } else
            {
                Debug.Log($"{player.PlayerName} bet {string.Join(", ", player.PlayerBet)}");
            }
        }
    }


    public void ResetBets()
    {
        foreach (PlayerData player in PlayerManager.Instance.Players)
        {
            player.PlayerBet.Clear();
        }
        Debug.Log("All bets reset.");
    }
}
