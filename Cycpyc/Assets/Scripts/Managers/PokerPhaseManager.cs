using System.Collections.Generic;
using Assets.Scripts.Managers;
using UnityEngine;

public class PokerPhaseManager : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private int maxTableCards = 5;
    [SerializeField] private int maxCardsPerPlayer = 5;

    private PokerPhaseStep currentPhaseStep = PokerPhaseStep.InitialDeal;

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
                Debug.Log("Final round reached. Evaluate results or proceed to next phase.");
                currentPhaseStep = PokerPhaseStep.Completed;
                break;
            case PokerPhaseStep.Completed:
                Debug.Log("Poker phase already completed.");
                GameManager.Instance.SetState(GameState.ArenaPhase);
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
        // - Показ UI панелей ставок

        BetManager.Instance.StartBetting();
    }

    public void ContinuePokerPhase() {
        AddingCardsToEntities();
        // - Показ UI панелей ставок
    }

    public void AddingCardsToEntities() {
        AddCardToPlayers();
        AddCardToTable();
    }

    public void InstantiateDeck()
    {
        deckManager.LoadDeck();
        deckManager.ShuffleDeck();

        List<CardDataSO> ActualCardDeck = deckManager.GetDeck();
    }

    /// <summary>
    /// Overload method for mvp 2.0
    /// </summary>
    /// <param name="selectedDeck">get selected deck by host</param>
    public void StartPokerPhase(DeckDataSO selectedDeck)
    {
        GameManager.Instance.SetState(GameState.PokerPhase);

        Debug.Log("Poker Phase started. GameState updated to PokerPhase.");

        // Тут можеш додати додаткову ініціалізацію:
        // - Завантаження колоди
        InstantiateDeck(selectedDeck);
        // - Роздачу карт
        // - Показ UI панелей ставок
    }

    /// <summary>
    /// Overload method for mvp 2.0
    /// </summary>
    /// <param name="selectedDeck">get selected deck by host</param>
    public void InstantiateDeck(DeckDataSO selectedDeck) {
        deckManager.LoadDeck(selectedDeck);
        deckManager.ShuffleDeck();

        List<CardDataSO> ActualCardDeck = deckManager.GetDeck();
    }

    private void DistributeCardsToPlayers(int cardsPerPlayer = 2)
    {  
        foreach (var player in PlayerManager.Instance.Players)
        {
            player.SelectedCards.Clear();

            for (int i = 0; i < cardsPerPlayer; i++)
            {
                var card = deckManager.DrawCard();
                if (card != null)
                {
                    player.SelectedCards.Add(card);
                }
            }

            Debug.Log($"Player {player.PlayerName} received:");
            foreach (var card in player.SelectedCards)
            {
                Debug.Log($"- {card.cardName} ({card.cardFigureShape}, {card.cardFigureColor})");
            }
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
                continue; // Пропустити цього гравця
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

    public void PausePhase()
    {
        GameManager.Instance.PauseGame();
    }

    public void ResumePhase()
    {
        GameManager.Instance.ResumeGame();
    }
}
