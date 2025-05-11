using UnityEngine;

[CreateAssetMenu(menuName = "Cards/UniqueCardProperties")]
public class UniqueCardPropertiesSO : ScriptableObject
{
    [Tooltip("Special effect name or description for this unique card.")]
    public string effectName; //mb create enum too? 

    [Tooltip("Detailed description of the unique effect.")]
    public string effectDescription;

    [Tooltip("Optional icon or visual for the unique effect.")]
    public Sprite effectIcon;
}
