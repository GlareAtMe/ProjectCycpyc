using System;
using System.Collections.Generic;
using Assets.Scripts.Enums;

[System.Serializable]
public class PlayerData
{
    public string PlayerId { get; private set; }
    public string PlayerName;

    public List<CardDataSO> SelectedCards = new List<CardDataSO>();
    public List<BetPunishments> PlayerAvailableBets = new List<BetPunishments>();
    public List<BetPunishments> PlayerBet = new List<BetPunishments>();
    
    public PlayerState State = PlayerState.Waiting;

    // Placeholder for future expansions
    // public PlayerCharacteristics Characteristics;
    // public List<PunishmentSO> AvailablePunishments;

    public PlayerData(string playerName)
    {
        PlayerId = playerName;
        PlayerName = playerName;

        //TODO REMAKE WITH SO (BetSetSO)
        PlayerAvailableBets = new List<BetPunishments> { 
            BetPunishments.DeadlyPunishment,  
            BetPunishments.HardPunishment,
            BetPunishments.HardPunishment,
            BetPunishments.MidllePunishment,
            BetPunishments.MidllePunishment,
            BetPunishments.MidllePunishment,
            BetPunishments.MidllePunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment,
            BetPunishments.EasyPunishment
        };
    }

    public PlayerData(Guid playerId, string playerName)
    {
        PlayerId = playerId.ToString();
        PlayerName = playerName;
    }
}
