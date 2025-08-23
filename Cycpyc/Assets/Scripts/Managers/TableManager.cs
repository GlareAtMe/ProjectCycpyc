using System;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private int maxCardsOnTable = 5;

    // Backing list is private; expose read-only view
    private readonly List<CardDataSO> cardsOnTable = new List<CardDataSO>();
    public IReadOnlyList<CardDataSO> CardsOnTable => cardsOnTable;

    // Optional: notify listeners (UI, effects) when table changes
    public event Action<IReadOnlyList<CardDataSO>> OnTableChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Add a card onto the table if capacity allows.</summary>
    public bool PlaceCardOnTable(CardDataSO card)
    {
        if (card == null)
        {
            Debug.LogWarning("TableManager.PlaceCardOnTable: card is null.");
            return false;
        }

        if (cardsOnTable.Count >= maxCardsOnTable)
        {
            Debug.Log("TableManager: maximum number of cards on the table reached.");
            return false;
        }

        cardsOnTable.Add(card);
        Debug.Log($"Card {card.cardName} placed on table. Total: {cardsOnTable.Count}");
        OnTableChanged?.Invoke(cardsOnTable);
        return true;
    }

    /// <summary>Remove a specific card from the table (first match).</summary>
    public bool RemoveCardFromTable(CardDataSO card)
    {
        if (card == null) return false;
        var removed = cardsOnTable.Remove(card);
        if (removed)
        {
            Debug.Log($"Card {card.cardName} removed from table. Total: {cardsOnTable.Count}");
            OnTableChanged?.Invoke(cardsOnTable);
        }
        return removed;
    }

    /// <summary>Remove the last placed card (LIFO), if any.</summary>
    public CardDataSO PopLastCard()
    {
        if (cardsOnTable.Count == 0) return null;
        var idx = cardsOnTable.Count - 1;
        var card = cardsOnTable[idx];
        cardsOnTable.RemoveAt(idx);
        Debug.Log($"Card {card.cardName} popped from table. Total: {cardsOnTable.Count}");
        OnTableChanged?.Invoke(cardsOnTable);
        return card;
    }

    /// <summary>Clear all cards from the table.</summary>
    public void ClearTable()
    {
        if (cardsOnTable.Count == 0)
        {
            Debug.Log("Table already empty.");
            return;
        }
        var count = cardsOnTable.Count;
        cardsOnTable.Clear();
        Debug.Log($"Table cleared. Removed {count} card(s).");
        OnTableChanged?.Invoke(cardsOnTable);
    }

    /// <summary>Optionally adjust max capacity at runtime.</summary>
    public void SetMaxCardsOnTable(int capacity)
    {
        maxCardsOnTable = Mathf.Max(0, capacity);
    }
}
