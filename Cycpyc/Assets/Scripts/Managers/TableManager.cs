using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance { get; private set; }

    public List<CardDataSO> CardsOnTable { get; private set; } = new List<CardDataSO>();

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

    public void PlaceCardOnTable(CardDataSO card)
    {
        CardsOnTable.Add(card);
        Debug.Log($"Card {card.cardName} placed on table.");
    }

    public void ClearTable()
    {
        CardsOnTable.Clear();
        Debug.Log("Table cleared.");
    }
}
