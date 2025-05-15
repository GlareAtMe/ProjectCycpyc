using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private DeckDataSO defaultDeckData;
    private List<CardDataSO> deck;

    private void Awake()
    {
        LoadDeck();
    }

    public void LoadDeck()
    {
        LoadDeck(defaultDeckData);
    }

    public void LoadDeck(DeckDataSO deckDataToLoad)
    {
        deck = new List<CardDataSO>(deckDataToLoad.cardsDeck);
        Debug.Log($"Deck loaded with {deck.Count} cards.");
    }

    public CardDataSO DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            return null;
        }

        CardDataSO drawnCard = deck[0];
        deck.RemoveAt(0);
        Debug.Log($"Drew card: {drawnCard.name}");
        return drawnCard;
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardDataSO temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        Debug.Log("Deck shuffled.");
    }

    public List<CardDataSO> GetDeck()
    {
        return deck;
    }
}
