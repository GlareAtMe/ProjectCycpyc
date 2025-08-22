using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Assets.Scripts.DataModels.BetData; // DTO: PerPlayerResolution, RoundComputationResult, BetSnapshot, RoundResult, BetResolutionEntry
using Assets.Scripts.Enums;

public class BetManager : MonoBehaviour
{
    #region Events

    public event Action OnBettingStarted;                         // відкрилась фаза ставок
    public event Action<float> OnBettingTick;                     // оновлення лічильника (кожен кадр або рідше)
    public event Action OnBettingTimeUp;                          // таймер вичерпано
    public event Action<PlayerData, BetPunishments> OnBetPlaced;  // гравець поставив ставку
    public event Action<PlayerData, BetPunishments> OnBetCanceled;// гравець скасував ставку
    public event Action OnBetsConfirmed;                          // ставки зафіксовані
    public event Action<RoundResult> OnBetsResolved;              // застосовано результати

    #endregion

    public static BetManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private float defaultBetDurationSeconds = 20f; // TODO: винести у ScriptableObject PokerConfig

    public BetRoundPhase Phase { get; private set; } = BetRoundPhase.None;
    public int CurrentRoundIndex { get; private set; } = 0;

    public float TimeLeft { get; private set; } = 0f;
    private Coroutine betTimerCoroutine;

    // знімок ставок, зроблений під час ConfirmBets
    private BetSnapshot lastBetSnapshot;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Public API — Betting Window

    /// <summary>Відкриває вікно ставок, скидає ставки, запускає таймер.</summary>
    public void StartBetting(float? durationSeconds = null)
    {
        if (Phase == BetRoundPhase.BettingOpen) return;

        ResetBets(); // очищає тільки PlayerBet (не чіпає ActivePunishments)
        Phase = BetRoundPhase.BettingOpen;

        var duration = Mathf.Max(0.1f, durationSeconds ?? defaultBetDurationSeconds);
        if (betTimerCoroutine != null) StopCoroutine(betTimerCoroutine);
        betTimerCoroutine = StartCoroutine(CoBetTimer(duration));

        Debug.Log($"[BetManager] Round {CurrentRoundIndex} — betting started ({duration:F1}s).");
        OnBettingStarted?.Invoke();
    }

    /// <summary>Ставка гравця. Повертає true якщо успіх.</summary>
    public bool PlaceBet(PlayerData player, BetPunishments bet)
    {
        if (!CanMutateBets()) return false;
        if (player == null) return false;

        if (!player.PlayerAvailableBets.Contains(bet))
        {
            Debug.LogWarning($"[BetManager] {player.PlayerName} tried invalid bet: {bet}");
            return false;
        }
        if (player.PlayerBet.Contains(bet)) return true; // ідемпотентність

        player.PlayerBet.Add(bet);
        player.PlayerAvailableBets.Remove(bet);
        Debug.Log($"[BetManager] {player.PlayerName} placed {bet}");
        OnBetPlaced?.Invoke(player, bet);
        return true;
    }

    /// <summary>Скасування ставки гравця поки вікно відкрите.</summary>
    public bool CancelBet(PlayerData player, BetPunishments bet)
    {
        if (!CanMutateBets()) return false;
        if (player == null) return false;

        if (player.PlayerBet.Remove(bet))
        {
            if (!player.PlayerAvailableBets.Contains(bet))
                player.PlayerAvailableBets.Add(bet);

            Debug.Log($"[BetManager] {player.PlayerName} canceled {bet}");
            OnBetCanceled?.Invoke(player, bet);
            return true;
        }
        return false;
    }

    /// <summary>Фіксує ставки, створює снімок і зупиняє таймер.</summary>
    public void ConfirmBets()
    {
        if (Phase != BetRoundPhase.BettingOpen) return;

        Phase = BetRoundPhase.BettingLocked;
        StopTimer();

        lastBetSnapshot = MakeBetSnapshot();
        LogConfirmedBets(lastBetSnapshot);

        OnBetsConfirmed?.Invoke();
    }

    #endregion

    #region Public API — Resolution (Compute vs Apply)

    /// <summary>Обчислює результат раунду (без мутації стану гравців).</summary>
    public RoundComputationResult ComputeRoundResult(List<PlayerData> standings)
    {
        if (Phase != BetRoundPhase.BettingLocked)
        {
            Debug.LogWarning($"[BetManager] ComputeRoundResult in wrong phase: {Phase}");
            return null;
        }
        if (standings == null || standings.Count == 0)
        {
            Debug.LogWarning("[BetManager] ComputeRoundResult: empty standings");
            return null;
        }
        if (lastBetSnapshot == null)
        {
            Debug.LogWarning("[BetManager] ComputeRoundResult: no bet snapshot");
            return null;
        }

        var winnerPlayer = standings[0];
        var perPlayerResolutions = new List<PerPlayerResolution>(standings.Count);

        foreach (var player in standings)
        {
            // беремо ставки із snapshot і робимо копію для безпечної історії
            lastBetSnapshot.BetsByPlayerId.TryGetValue(player.PlayerId, out var placedBetsRaw);
            var placedBets = placedBetsRaw != null
                ? new List<BetPunishments>(placedBetsRaw)
                : new List<BetPunishments>();

            // для переможця — порожній список, для решти — їхні ж ставки стають покараннями
            IReadOnlyList<BetPunishments> punishmentsToApply =
                (player == winnerPlayer) ? Array.Empty<BetPunishments>() : placedBets;

            perPlayerResolutions.Add(
                new PerPlayerResolution(
                    player.PlayerId,
                    player == winnerPlayer,
                    placedBets,
                    punishmentsToApply
                )
            );
        }

        return new RoundComputationResult(
            lastBetSnapshot.RoundIndex,
            winnerPlayer.PlayerId,
            perPlayerResolutions
        );
    }

    /// <summary>Застосовує результат (мутує ActivePunishments/Stats), емить івенти, готує наступний раунд.</summary>
    public RoundResult ApplyRoundResult(RoundComputationResult roundComputationResult)
    {
        if (roundComputationResult == null) return null;

        var roundResult = new RoundResult
        {
            RoundIndex = roundComputationResult.RoundIndex,
            UtcResolvedAt = DateTime.UtcNow
        };

        foreach (var entry in roundComputationResult.Players)
        {
            var playerData = PlayerManager.Instance.GetPlayerById(entry.PlayerId);
            if (playerData == null) continue;

            var placedBetsCopy = entry.BetsPlaced?.ToList() ?? new List<BetPunishments>();

            // створюємо порожній список applied — наповнимо його нижче тільки один раз
            var resultEntry = new BetResolutionEntry(
                entry.PlayerId,
                entry.IsWinner,
                new List<BetPunishments>(),
                placedBetsCopy
            );

            if (entry.IsWinner)
            {
                playerData.Stats?.RegisterWin(roundComputationResult.RoundIndex);
                // TODO(optional): нагорода переможцю або зняття 1 активного покарання
            } else
            {
                var punishments = entry.PunishmentsToApply ?? Array.Empty<BetPunishments>();
                foreach (var punishment in punishments)
                {
                    if (!playerData.ActivePunishments.Contains(punishment))
                        playerData.ActivePunishments.Add(punishment);

                    // додаємо в результат тільки тут (жодних дублікатів)
                    resultEntry.AppliedPunishments.Add(punishment);
                }

                playerData.Stats?.RegisterLoss(
                    roundComputationResult.RoundIndex,
                    placedBetsCopy
                );
            }

            roundResult.Entries.Add(resultEntry);
        }

        Phase = BetRoundPhase.Resolved;
        Debug.Log($"[BetManager] Round {roundComputationResult.RoundIndex} resolved at {roundResult.UtcResolvedAt:O}");

        OnBetsResolved?.Invoke(roundResult);

        PrepareNextRound();
        return roundResult;
    }

    /// <summary>Оркестратор: Compute → Apply.</summary>
    public RoundResult ResolveBets(List<PlayerData> standings)
    {
        var roundComputationResult = ComputeRoundResult(standings);
        if (roundComputationResult == null) return null;
        return ApplyRoundResult(roundComputationResult);
    }

    #endregion

    #region Helpers

    private bool CanMutateBets() => Phase == BetRoundPhase.BettingOpen;

    private BetSnapshot MakeBetSnapshot()
    {
        var map = new Dictionary<string, List<BetPunishments>>();
        foreach (var player in PlayerManager.Instance.Players)
        {
            // якщо раптом дубль id — перезапишемо останнім (або можна TryAdd із логом)
            map[player.PlayerId] = new List<BetPunishments>(player.PlayerBet);
        }
        return new BetSnapshot(CurrentRoundIndex, map);
    }

    private void LogConfirmedBets(BetSnapshot snapshot)
    {
        Debug.Log("[BetManager] Bets confirmed:");
        foreach (var player in PlayerManager.Instance.Players)
        {
            var has = snapshot.BetsByPlayerId.TryGetValue(player.PlayerId, out var bets);
            if (!has || bets == null || bets.Count == 0)
                Debug.Log($" - {player.PlayerName}: no bets");
            else
                Debug.Log($" - {player.PlayerName}: {string.Join(", ", bets)}");
        }
    }

    /// <summary>Скидає лише ставки (PlayerBet). ActivePunishments не чіпаємо.</summary>
    public void ResetBets()
    {
        foreach (var player in PlayerManager.Instance.Players)
            player.PlayerBet.Clear();

        Debug.Log("[BetManager] Bets reset.");
    }

    private void PrepareNextRound()
    {
        // Підготовка до наступного раунду: інкремент, очистка transient-даних
        CurrentRoundIndex += 1;

        foreach (var player in PlayerManager.Instance.Players)
            player.PlayerBet.Clear();

        lastBetSnapshot = null;
        StopTimer();    // страхуємо зависання таймера
        TimeLeft = 0f;  // синхронізація з UI
        Phase = BetRoundPhase.None; // Нова фаза відкривається PokerPhaseManager.StartBetting()
    }

    #endregion

    #region Timer

    private System.Collections.IEnumerator CoBetTimer(float duration)
    {
        TimeLeft = duration;
        while (TimeLeft > 0f && Phase == BetRoundPhase.BettingOpen)
        {
            yield return null;
            TimeLeft -= Time.deltaTime;
            OnBettingTick?.Invoke(Mathf.Max(0f, TimeLeft));
        }

        // Якщо все ще відкрита фаза — час вийшов
        if (Phase == BetRoundPhase.BettingOpen)
        {
            OnBettingTimeUp?.Invoke();
            ConfirmBets(); // авто-фіксація
        }

        betTimerCoroutine = null;
    }

    private void StopTimer()
    {
        if (betTimerCoroutine != null) { StopCoroutine(betTimerCoroutine); betTimerCoroutine = null; }
        TimeLeft = 0f;
    }

    #endregion
}
