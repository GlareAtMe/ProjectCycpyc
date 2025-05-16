using UnityEngine;

[CreateAssetMenu(menuName = "Cards/CardFigureSprite")]
public class CardFigureSpriteSO : ScriptableObject
{
    [Tooltip("Icon representing the figure on the card.")]
    public Sprite figureIcon;

    [Tooltip("Optional color tint applied to the figure.")]
    public Color figureColor = Color.black;
}
