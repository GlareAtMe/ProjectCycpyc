using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private DeckDataSO defaultDeckData;

    private List<CardDataSO> deck;
    private int topIndex = 0; // pointer to the current top card

    private void Awake()
    {
        if (defaultDeckData != null) LoadDeck();
        else Debug.LogWarning("DeckManager: defaultDeckData is not assigned.");
    }

    public void LoadDeck()
    {
        LoadDeck(defaultDeckData);
    }

    public void LoadDeck(DeckDataSO deckDataToLoad)
    {
        if (deckDataToLoad == null || deckDataToLoad.cardsDeck == null)
        {
            Debug.LogError("DeckManager: deckDataToLoad or its cardsDeck is null.");
            deck = new List<CardDataSO>();
            topIndex = 0;
            return;
        }

        deck = new List<CardDataSO>(deckDataToLoad.cardsDeck);
        topIndex = 0;
        Debug.Log($"Deck loaded with {deck.Count} cards.");
    }

    public void ShuffleDeck()
    {
        if (deck == null || deck.Count == 0)
        {
            Debug.LogWarning("DeckManager: no deck to shuffle.");
            return;
        }

        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count); // upper bound exclusive (int)
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
        topIndex = 0; // after shuffle we start drawing from the beginning
        Debug.Log("Deck shuffled.");
    }

    public CardDataSO DrawCard()
    {
        if (deck == null || topIndex >= deck.Count)
        {
            Debug.LogWarning("Deck is empty!");
            return null;
        }

        CardDataSO drawnCard = deck[topIndex];
        topIndex++;
        Debug.Log($"Drew card: {drawnCard.name}");
        return drawnCard;
    }

    public List<CardDataSO> DrawCards(int count)
    {
        var result = new List<CardDataSO>(count);
        for (int i = 0; i < count; i++)
        {
            var c = DrawCard();
            if (c == null) break;
            result.Add(c);
        }
        return result;
    }

    public int RemainingCards => (deck == null) ? 0 : (deck.Count - topIndex);

    public IReadOnlyList<CardDataSO> GetDeck()
    {
        return deck;
    }

    public void ResetToDefaultDeck()
    {
        if (defaultDeckData == null)
        {
            Debug.LogWarning("DeckManager: no default deck to reset.");
            return;
        }
        LoadDeck(defaultDeckData);
        ShuffleDeck();
    }
}
