using UnityEngine;

public class TestDeckManager : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;

    [Header("Available Decks")]
    [SerializeField] private DeckDataSO defaultDeck;
    [SerializeField] private DeckDataSO specialDeck;

    public void LoadDefaultDeck()
    {
        Debug.Log("Loading Default Deck...");
        deckManager.LoadDeck(defaultDeck);
    }

    public void LoadSpecialDeck()
    {
        Debug.Log("Loading Special Deck...");
        deckManager.LoadDeck(specialDeck);
    }

    public void ShuffleDeck()
    {
        Debug.Log("Shuffling Deck...");
        deckManager.ShuffleDeck();
    }

    public void DrawCard()
    {
        var card = deckManager.DrawCard();
        if (card != null)
        {
        } else
        {
            Debug.LogWarning("No card drawn. Deck may be empty.");
        }
    }

    public void PrintDeck()
    {
        Debug.Log("Listing cards in current deck:");
        foreach (var card in deckManager.GetDeck())
        {
            Debug.Log("- " + card.cardName);
        }
    }
}
