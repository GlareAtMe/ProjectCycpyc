using UnityEngine;

public class TestPokerPhaseController : MonoBehaviour
{
    [SerializeField] private PokerPhaseManager pokerPhaseManager;

    public void TestDistributeCards()
    {
        pokerPhaseManager.InstantiateDeck();
    }
}
