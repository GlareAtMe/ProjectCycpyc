using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private List<PlayerData> _players = new();
    private readonly Dictionary<string, PlayerData> _playersById = new();

    public IReadOnlyList<PlayerData> Players => _players;

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
    }

    public bool RegisterPlayer(PlayerData p)
    {
        if (p == null || string.IsNullOrEmpty(p.PlayerId)) return false;
        if (_playersById.ContainsKey(p.PlayerId))
        {
            Debug.LogWarning($"[PlayerManager] Duplicate PlayerId: {p.PlayerId}");
            return false;
        }
        _players.Add(p);
        _playersById[p.PlayerId] = p;
        return true;
    }

    public bool UnregisterPlayer(string playerId)
    {
        if (!_playersById.TryGetValue(playerId, out var p)) return false;
        _playersById.Remove(playerId);
        _players.Remove(p);
        return true;
    }

    public void RebuildIndex()
    {
        _playersById.Clear();
        foreach (var p in _players)
            _playersById[p.PlayerId] = p;
    }

    public PlayerData GetPlayerById(string playerId)
    {
        _playersById.TryGetValue(playerId, out var p);
        return p;
    }
}
