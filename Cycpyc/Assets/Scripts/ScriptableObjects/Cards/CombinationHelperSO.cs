using UnityEngine;

[CreateAssetMenu(menuName = "Cards/CombinationHelper")]
public class CombinationHelperSO : ScriptableObject
{
    [Tooltip("Describes how this card interacts with combinations or synergies.")]
    public string combinationDescription;

    // need update logic later
}
