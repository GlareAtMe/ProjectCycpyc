using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Enums;

[Serializable]
public class PlayerData
{
    // Ідентифікація
    public string PlayerId { get; private set; }
    public string PlayerName { get; set; }

    // Стан у грі
    public PlayerState State { get; set; } = PlayerState.Waiting;
    public bool IsReady { get; private set; } = false;

    // Карти (покер-фаза)
    public List<CardDataSO> SelectedCards { get; private set; } = new List<CardDataSO>();

    // Ставки: доступні у цьому матчі та вибрані на поточний раунд
    public List<BetPunishments> PlayerAvailableBets { get; private set; } = new List<BetPunishments>();
    public List<BetPunishments> PlayerBet { get; private set; } = new List<BetPunishments>();

    // Застосовані покарання, що переносяться між раундами
    public List<BetPunishments> ActivePunishments { get; private set; } = new List<BetPunishments>();

    // Статистика (мінімально)
    public PlayerStats Stats { get; private set; } = new PlayerStats();

    // Додаткові метадані (за потреби)
    public DateTime LastStateChangeUtc { get; private set; } = DateTime.UtcNow;

    #region Конструктори / фабрики

    // Якщо ми поки що ідентифікуємо гравця іменем
    public PlayerData(string playerName)
    {
        PlayerId = playerName;         // тимчасово: id = name
        PlayerName = playerName;
    }

    // Якщо є зовнішній GUID (наприклад, мережева ідентифікація)
    public PlayerData(Guid playerId, string playerName)
    {
        PlayerId = playerId.ToString();
        PlayerName = playerName;
    }

    // Фабрика з BetSetSO 
    public static PlayerData Create(string playerName, BetSetSO betSet)
    {
        var p = new PlayerData(playerName);
        if (betSet != null)
            p.PlayerAvailableBets = betSet.GetInitialBets(); // копія з SO
        return p;
    }

    #endregion

    #region API керування станом

    public void SetReady(bool ready)
    {
        if (IsReady == ready) return;
        IsReady = ready;
        Touch();
    }

    public void SetState(PlayerState newState)
    {
        if (State == newState) return;
        State = newState;
        Touch();
    }

    // Очищує транзієнтні дані раунду (викликає BetManager при підготовці до нового раунду)
    public void ResetTransientForNewRound()
    {
        PlayerBet.Clear();
        IsReady = false;
        // PlayerAvailableBets відновлюємо за правилами матчу окремо (див. BetSetSO/матч-правила)
        Touch();
    }

    // Ініціалізація пулу доступних ставок із SO або зовнішнього конфігу
    public void InitializeAvailableBets(IEnumerable<BetPunishments> source)
    {
        PlayerAvailableBets = source != null ? new List<BetPunishments>(source) : new List<BetPunishments>();
        Touch();
    }

    // Утиліта: безпечне додавання в ActivePunishments
    public void AddActivePunishments(IEnumerable<BetPunishments> punishments)
    {
        if (punishments == null) return;
        foreach (var pun in punishments)
            if (!ActivePunishments.Contains(pun))
                ActivePunishments.Add(pun);
        Touch();
    }

    // Для дебагу/логів
    public override string ToString() => $"{PlayerName} ({PlayerId})";

    private void Touch() => LastStateChangeUtc = DateTime.UtcNow;

    #endregion
}

[Serializable]
public class PlayerStats
{
    public int Wins { get; private set; }
    public int Losses { get; private set; }

    public void RegisterWin(int roundIndex) { Wins++; }
    public void RegisterLoss(int roundIndex, List<BetPunishments> placedBets) { Losses++; }
}
