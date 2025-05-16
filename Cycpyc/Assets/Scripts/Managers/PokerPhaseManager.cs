using System.Collections.Generic;
using Assets.Scripts.Managers;
using UnityEngine;

public class PokerPhaseManager : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;

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

    public void StartPokerPhase()
    {
        GameManager.Instance.SetState(GameState.PokerPhase);

        Debug.Log("Poker Phase started. GameState updated to PokerPhase.");

        InstantiateDeck();        
        DistributeCardsToPlayers();
        PlaceCardsToTable(2);//fix
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

    public void InstantiateDeck()
    {
        deckManager.LoadDeck();
        deckManager.ShuffleDeck();

        List<CardDataSO> ActualCardDeck = deckManager.GetDeck();
    }

    private void DistributeCardsToPlayers()
    {
        int cardsPerPlayer = 2;  // Кількість карт на гравця

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

            // Вивід результату для гравця
            Debug.Log($"Player {player.PlayerName} received:");
            foreach (var card in player.SelectedCards)
            {
                Debug.Log($"- {card.cardName} ({card.cardFigureShape}, {card.cardFigureColor})");
            }
        }
    }

    private void PlaceCardsToTable(int cardsOnTableCount)
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


    public void PausePhase()
    {
        GameManager.Instance.PauseGame();
    }

    public void ResumePhase()
    {
        GameManager.Instance.ResumeGame();
    }
}
