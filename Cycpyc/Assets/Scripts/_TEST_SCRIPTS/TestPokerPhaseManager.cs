using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataModels.BetData;
using Assets.Scripts.Enums;
using UnityEngine;

public class TestPokerPhaseController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private int playersToBoot = 2;
    [SerializeField] private float testBetWindowSeconds = 20f;

    private void OnEnable()
    {
        // subscribe to bet events for clear logs
        if (BetManager.Instance != null)
        {
            BetManager.Instance.OnBettingStarted += HandleBettingStarted;
            //BetManager.Instance.OnBettingTick += HandleBettingTick;
            BetManager.Instance.OnBettingTimeUp += HandleBettingTimeUp;
            BetManager.Instance.OnBetsConfirmed += HandleBetsConfirmed;
            BetManager.Instance.OnBetsResolved += HandleBetsResolved;
            BetManager.Instance.OnBetPlaced += HandleBetPlaced;
            BetManager.Instance.OnBetCanceled += HandleBetCanceled;
        }
    }

    private void OnDisable()
    {
        if (BetManager.Instance != null)
        {
            BetManager.Instance.OnBettingStarted -= HandleBettingStarted;
            //BetManager.Instance.OnBettingTick -= HandleBettingTick;
            BetManager.Instance.OnBettingTimeUp -= HandleBettingTimeUp;
            BetManager.Instance.OnBetsConfirmed -= HandleBetsConfirmed;
            BetManager.Instance.OnBetsResolved -= HandleBetsResolved;
            BetManager.Instance.OnBetPlaced -= HandleBetPlaced;
            BetManager.Instance.OnBetCanceled -= HandleBetCanceled;
        }
    }

    // ---------- Boot / Phase ----------

    public void BootPlayers()
    {
        PlayerManager.Instance.ClearAllPlayers();
        PlayerManager.Instance.EnsureTestPlayers(Mathf.Max(2, playersToBoot));
        Debug.Log($"[Test] Booted {PlayerManager.Instance.Players.Count} players.");
    }

    public void StartPokerPhase()
    {
        FindFirstObjectByType<PokerPhaseManager>().StartPokerPhase();
        Debug.Log("[Test] PokerPhase started.");
    }

    public void NextStep()
    {
        FindFirstObjectByType<PokerPhaseManager>().ProceedToNextStep();
        Debug.Log("[Test] Proceeded to next subround.");
    }

    public void EndPokerPhase()
    {
        FindFirstObjectByType<PokerPhaseManager>().EndPokerPhase();
        Debug.Log("[Test] EndPokerPhase called.");
    }

    // ---------- Betting window control ----------

    public void StartBettingFresh()       // reset previous bets
    {
        BetManager.Instance.StartBetting(testBetWindowSeconds, resetPlayerBets: true);
        Debug.Log("[Test] StartBetting(reset=true).");
    }

    public void StartBettingCumulative()  // keep previous bets
    {
        BetManager.Instance.StartBetting(testBetWindowSeconds, resetPlayerBets: false);
        Debug.Log("[Test] StartBetting(reset=false).");
    }

    public void ConfirmBets()
    {
        if (BetManager.Instance.Phase == BetRoundPhase.BettingOpen)
        {
            BetManager.Instance.ConfirmBets();
            Debug.Log("[Test] Forced ConfirmBets().");
        } else Debug.Log("[Test] Confirm skipped: not in BettingOpen phase.");
    }

    // ---------- Bets: place / cancel ----------

    public void PlaceRandomBetsOncePerPlayer()
    {
        if (BetManager.Instance.Phase != BetRoundPhase.BettingOpen)
        {
            Debug.Log("[Test] PlaceRandomBets skipped: betting not open.");
            return;
        }

        foreach (var p in PlayerManager.Instance.Players)
        {
            var avail = p.PlayerAvailableBets;
            if (avail == null || avail.Count == 0)
            {
                Debug.Log($"[Test] {p.PlayerName}: no available bets.");
                continue;
            }
            var bet = avail[Random.Range(0, avail.Count)];
            BetManager.Instance.PlaceBet(p, bet);
        }
    }

    public void PlaceAllAvailableBetsPerPlayer()
    {
        if (BetManager.Instance.Phase != BetRoundPhase.BettingOpen) return;

        foreach (var p in PlayerManager.Instance.Players)
        {
            var snapshot = new List<BetPunishments>(p.PlayerAvailableBets);
            foreach (var bet in snapshot)
                BetManager.Instance.PlaceBet(p, bet);
            Debug.Log($"[Test] {p.PlayerName}: placed {snapshot.Count} bets.");
        }
    }

    public void CancelRandomBetForEachPlayer()
    {
        if (BetManager.Instance.Phase != BetRoundPhase.BettingOpen) return;

        foreach (var p in PlayerManager.Instance.Players)
        {
            if (p.PlayerBet.Count == 0) continue;
            var bet = p.PlayerBet[Random.Range(0, p.PlayerBet.Count)];
            BetManager.Instance.CancelBet(p, bet);
        }
    }

    public void ResetAllBets()
    {
        BetManager.Instance.ResetBets();
        Debug.Log("[Test] ResetBets called.");
    }

    // ---------- State print ----------

    public void PrintState()
    {
        var bm = BetManager.Instance;
        Debug.Log($"[State] Phase={bm.Phase} Round={bm.CurrentRoundIndex} TimeLeft={bm.TimeLeft:F1}");

        foreach (var p in PlayerManager.Instance.Players)
        {
            Debug.Log($" - {p.PlayerName} | Bets: {string.Join(", ", p.PlayerBet)} | Available: {string.Join(", ", p.PlayerAvailableBets)} | ActivePunishments: {string.Join(", ", p.ActivePunishments)} | Cards: {p.SelectedCards.Count}");
        }
        Debug.Log($"Table: {TableManager.Instance.CardsOnTable.Count} cards");
    }

    // ---------- Event handlers (logs) ----------

    private void HandleBettingStarted() => Debug.Log("[Evt] BettingStarted");
    //private void HandleBettingTick(float tl) => Debug.Log($"[Evt] BettingTick {tl:F1}s");
    private void HandleBettingTimeUp() => Debug.Log("[Evt] BettingTimeUp");
    private void HandleBetsConfirmed() => Debug.Log("[Evt] BetsConfirmed");
    private void HandleBetsResolved(RoundResult r) => Debug.Log($"[Evt] BetsResolved round={r.RoundIndex} entries={r.Entries.Count}");
    private void HandleBetPlaced(PlayerData p, BetPunishments b) => Debug.Log($"[Evt] BetPlaced {p.PlayerName}: {b}");
    private void HandleBetCanceled(PlayerData p, BetPunishments b) => Debug.Log($"[Evt] BetCanceled {p.PlayerName}: {b}");
}
