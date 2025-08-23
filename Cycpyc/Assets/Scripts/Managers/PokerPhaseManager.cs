using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Managers;
using Assets.Scripts.Enums;

public class PokerPhaseManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private int maxTableCards = 5;
    [SerializeField] private int maxCardsPerPlayer = 5;

    [Tooltip("Якщо не заданий — буде взято BetManager.Instance")]
    [SerializeField] private BetManager betManager;

    [Header("Betting")]
    [SerializeField] private float betWindowSeconds = 20f;
    [Tooltip("Відкривати вікно ставок після кожного додавання карт (Second/Third/Final)")]
    [SerializeField] private bool openBettingEverySubround = true;

    private PokerPhaseStep currentPhaseStep = PokerPhaseStep.InitialDeal;
    private bool isEndingPokerPhase = false; // ідемпотентність EndPokerPhase

    private void Awake()
    {
        if (betManager == null) betManager = BetManager.Instance;
    }

    private void OnEnable()
    {
        if (betManager == null) return;

        betManager.OnBettingStarted += HandleBettingStarted;
        betManager.OnBettingTick += HandleBettingTick;
        betManager.OnBettingTimeUp += HandleBettingTimeUp;
        betManager.OnBetsConfirmed += HandleBetsConfirmed;
        betManager.OnBetsResolved += HandleBetsResolved;
    }

    private void OnDisable()
    {
        if (betManager == null) return;

        betManager.OnBettingStarted -= HandleBettingStarted;
        betManager.OnBettingTick -= HandleBettingTick;
        betManager.OnBettingTimeUp -= HandleBettingTimeUp;
        betManager.OnBetsConfirmed -= HandleBetsConfirmed;
        betManager.OnBetsResolved -= HandleBetsResolved;
    }

    public void ProceedToNextStep()
    {
        switch (currentPhaseStep)
        {
            case PokerPhaseStep.InitialDeal:
                StartPokerPhase();
                break;

            case PokerPhaseStep.SecondRound:
                ContinuePokerPhase();
                currentPhaseStep = PokerPhaseStep.ThirdRound;
                break;

            case PokerPhaseStep.ThirdRound:
                ContinuePokerPhase();
                currentPhaseStep = PokerPhaseStep.FinalRound;
                break;

            case PokerPhaseStep.FinalRound:
                // Останній підраунд: закінчуємо покерну фазу централізовано
                EndPokerPhase();
                currentPhaseStep = PokerPhaseStep.Completed;
                break;

            case PokerPhaseStep.Completed:
                Debug.Log("Poker phase already completed.");
                // Тут буде перехід у наступну фазу/сцену (арена) — робимо це поза цим методом
                break;
        }
    }

    public void StartPokerPhase()
    {
        GameManager.Instance.SetState(GameState.PokerPhase);
        Debug.Log("Poker Phase started. GameState updated to PokerPhase.");

        InstantiateDeck();
        DistributeCardsToPlayers();
        PlaceCardsToTable();

        currentPhaseStep = PokerPhaseStep.SecondRound;

        // Перше вікно ставок — чистий початок (скидання ставок робить сам BetManager при reset=true)
        StartBettingWindow(resetPlayerBets: true);
    }

    public void ContinuePokerPhase()
    {
        // Додаємо карти гравцям і на стіл
        AddingCardsToEntities();

        // Відкриваємо наступне вікно ставок (кумулятивно — без скидання)
        if (openBettingEverySubround)
            StartBettingWindow(resetPlayerBets: false);
    }

    /// <summary>Єдина точка завершення покер-фази: гарантує Confirm → standings → Resolve → перехід далі.</summary>
    public void EndPokerPhase()
    {
        if (isEndingPokerPhase) return; // захист від двох викликів
        isEndingPokerPhase = true;

        // 1) Якщо останнє вікно ставок ще відкрите — зафіксувати
        if (betManager != null && betManager.Phase == BetRoundPhase.BettingOpen)
            betManager.ConfirmBets();

        // 2) standings (переможець першим)
        var standings = ComputeStandings();
        if (standings == null || standings.Count == 0)
        {
            Debug.LogWarning("[PokerPhaseManager] EndPokerPhase: standings are empty.");
            isEndingPokerPhase = false;
            return;
        }

        // 3) Resolve покарань/статистики
        var roundResult = betManager.ResolveBets(standings);
        if (roundResult == null)
        {
            Debug.LogWarning("[PokerPhaseManager] EndPokerPhase: ResolveBets returned null.");
            isEndingPokerPhase = false;
            return;
        }

        // 4) Зміна стану гри / перехід у наступну сцену (арена)
        GameManager.Instance.SetState(GameState.ArenaPhase);

        // TODO: передати roundResult у GameSession і завантажити сцену арени:
        // GameSession.Instance.SetLastRoundResult(roundResult);
        // SceneLoader.Instance.LoadArenaScene();

        isEndingPokerPhase = false;
    }

    // ---------- Helpers ----------

    private void StartBettingWindow(bool resetPlayerBets)
    {
        if (betManager == null) return;

        // якщо попереднє вікно ще відкрите — зафіксуємо перед новим
        if (betManager.Phase == BetRoundPhase.BettingOpen)
            betManager.ConfirmBets();

        // викликаємо перевантаження StartBetting(duration, reset)
        // (переконайся, що у BetManager є такий метод; якщо ні — додай прапорець reset)
        betManager.StartBetting(betWindowSeconds, resetPlayerBets);
    }

    public void InstantiateDeck()
    {
        deckManager.LoadDeck();
        deckManager.ShuffleDeck();
        IReadOnlyList<CardDataSO> actualCardDeck = deckManager.GetDeck();
    }

    public void InstantiateDeck(DeckDataSO selectedDeck)
    {
        deckManager.LoadDeck(selectedDeck);
        deckManager.ShuffleDeck();
        IReadOnlyList<CardDataSO> actualCardDeck = deckManager.GetDeck();
    }

    private void DistributeCardsToPlayers(int cardsPerPlayer = 2)
    {
        foreach (var player in PlayerManager.Instance.Players)
        {
            player.SelectedCards.Clear();

            for (int i = 0; i < cardsPerPlayer; i++)
            {
                var card = deckManager.DrawCard();
                if (card != null) player.SelectedCards.Add(card);
            }

            Debug.Log($"Player {player.PlayerName} received:");
            foreach (var card in player.SelectedCards)
                Debug.Log($"- {card.cardName} ({card.cardFigureShape}, {card.cardFigureColor})");
        }
    }

    private void PlaceCardsToTable(int cardsOnTableCount = 2)
    {
        TableManager.Instance.ClearTable();

        for (int i = 0; i < cardsOnTableCount; i++)
        {
            var card = deckManager.DrawCard();
            if (card != null)
            {
                TableManager.Instance.PlaceCardOnTable(card);
                Debug.Log($"Placed on table: {card.cardName}");
            }
        }
    }

    private void AddingCardsToEntities()
    {
        AddCardToPlayers();
        AddCardToTable();
    }

    private void AddCardToTable()
    {
        if (TableManager.Instance.CardsOnTable.Count >= maxTableCards)
        {
            Debug.Log("Maximum number of cards on the table reached.");
            return;
        }

        var card = deckManager.DrawCard();
        if (card != null)
        {
            TableManager.Instance.PlaceCardOnTable(card);
            Debug.Log($"Added to table: {card.cardName}");
        }
    }

    private void AddCardToPlayers(int cardsPerPlayer = 1)
    {
        foreach (var player in PlayerManager.Instance.Players)
        {
            if (player.SelectedCards.Count >= maxCardsPerPlayer)
            {
                Debug.Log($"Player {player.PlayerName} already has the maximum number of cards.");
                continue;
            }

            for (int i = 0; i < cardsPerPlayer; i++)
            {
                var card = deckManager.DrawCard();
                if (card != null)
                {
                    player.SelectedCards.Add(card);
                    Debug.Log($"Player {player.PlayerName} received: {card.cardName} ({card.cardFigureShape}, {card.cardFigureColor})");
                }
            }
        }
    }

    // ---------- Події BetManager (для UI/логів, без жорсткої логіки переходів) ----------

    private void HandleBettingStarted()
    {
        // TODO: показати панель ставок, обнулити таймер у PokerUIManager
    }

    private void HandleBettingTick(float timeLeft)
    {
        // TODO: оновлювати таймер у PokerUIManager
    }

    private void HandleBettingTimeUp()
    {
        // TODO: мигнути/звук у PokerUIManager
    }

    private void HandleBetsConfirmed()
    {
        // Можна оновити підсумки ставок в UI — логіка переходу в EndPokerPhase()
    }

    private void HandleBetsResolved(Assets.Scripts.DataModels.BetData.RoundResult result)
    {
        // Тут можна показати короткий summary у покерній сцені (опційно),
        // але сам перехід відбувається в EndPokerPhase()
    }

    // ---------- Standings (заглушка; підстав свою оцінку рук) ----------

    private List<PlayerData> ComputeStandings()
    {
        var players = PlayerManager.Instance.Players.ToList();
        players.Sort((a, b) =>
        {
            int aScore = EvaluateHandScore(a);
            int bScore = EvaluateHandScore(b);
            return bScore.CompareTo(aScore); // більший score — вище
        });
        return players;
    }

    private int EvaluateHandScore(PlayerData player)
    {
        // TODO: замінити на реальну оцінку (Texas Hold'em + карти на столі)
        int baseScore = player.SelectedCards.Count + TableManager.Instance.CardsOnTable.Count;
        return baseScore;
    }
}
