using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enums;

[CreateAssetMenu(fileName = "BetSet", menuName = "Bet Set")]
public class BetSetSO : ScriptableObject
{
    [Tooltip("Початковий пул ставок гравця на матч (можна дублювати елементи для ваги).")]
    public List<BetPunishments> initialBets = new List<BetPunishments>();

    // Повертаємо копію, щоб не мутувати оригінал у рантаймі
    public List<BetPunishments> GetInitialBets()
    {
        return new List<BetPunishments>(initialBets);
    }
}
