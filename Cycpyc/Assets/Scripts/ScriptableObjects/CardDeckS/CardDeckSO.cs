using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Cards/DeckData")]
public class DeckDataSO : ScriptableObject
{
    [Tooltip("List of cards in this deck.")]
    public List<CardDataSO> cardsDeck;
}
