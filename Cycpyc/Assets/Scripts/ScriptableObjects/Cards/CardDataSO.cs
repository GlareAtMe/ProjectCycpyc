using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/CardData")]
public class CardDataSO : ScriptableObject
{
    [Tooltip("Card full name include Figure type and color")]
    public string cardName;

    [Tooltip("The main shape of the card: Sphere, Cube, Pyramid, etc.")]
    public CardFigureShape cardFigureShape;

    [Tooltip("The color of the figure on the card.")]
    public CardFigureColor cardFigureColor;

    [Tooltip("Bonus modifiers applied by this card.")]
    public CardBonusSO cardBonus;

    [Tooltip("Visual style for the card background and frame.")]
    public CardSpriteSO cardSprite;

    [Tooltip("Visual style for the figure displayed on the card.")]
    public CardFigureSpriteSO cardFigureSprite;

    [Tooltip("Helper data used for combination and synergy calculations.")]
    public CombinationHelperSO combinationHelper;

    [Tooltip("Check if this card is a unique type with special properties.")]
    public bool isCardUnique = false;

    [Tooltip("Additional properties for unique cards.")]
    public UniqueCardPropertiesSO uniqueProperties;
}
