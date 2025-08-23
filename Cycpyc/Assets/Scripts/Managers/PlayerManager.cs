using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private readonly List<PlayerData> players = new();
    private readonly Dictionary<string, PlayerData> playersById = new();

    public IReadOnlyList<PlayerData> Players => players;

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

    // ---- Core API ----

    public bool RegisterPlayer(PlayerData player)
    {
        if (player == null || string.IsNullOrEmpty(player.PlayerId)) return false;
        if (playersById.ContainsKey(player.PlayerId))
        {
            Debug.LogWarning($"[PlayerManager] Duplicate PlayerId: {player.PlayerId}");
            return false;
        }
        players.Add(player);
        playersById[player.PlayerId] = player;
        return true;
    }

    public bool UnregisterPlayer(string playerId)
    {
        if (!playersById.TryGetValue(playerId, out var player)) return false;
        playersById.Remove(playerId);
        players.Remove(player);
        return true;
    }

    public void RebuildIndex()
    {
        playersById.Clear();
        foreach (var player in players)
            playersById[player.PlayerId] = player;
    }

    public PlayerData GetPlayerById(string playerId)
    {
        playersById.TryGetValue(playerId, out var player);
        return player;
    }

    // ---- Test & utility helpers (for quick pokerscene runs) ----

    /// <summary>Create and register a player with a fresh Guid ID.</summary>
    public PlayerData CreateAndRegisterPlayer(string displayName)
    {
        var player = new PlayerData(Guid.NewGuid(), displayName);
        if (!RegisterPlayer(player))
        {
            Debug.LogError($"[PlayerManager] Failed to register player {displayName}");
            return null;
        }
        return player;
    }

    /// <summary>Remove all players and rebuild indices.</summary>
    public void ClearAllPlayers()
    {
        players.Clear();
        playersById.Clear();
    }

    /// <summary>Ensure at least N players exist (quick boot for tests).</summary>
    public void EnsureTestPlayers(int count)
    {
        if (count < 1) count = 1;
        while (players.Count < count)
        {
            var idx = players.Count + 1;
            CreateAndRegisterPlayer($"Player_{idx}");
        }
    }
}
