using UnityEngine;

public class TestGameManager : MonoBehaviour
{
    public void OnPauseButtonPressed()
    {
        GameManager.Instance.PauseGame();
    }

    public void OnResumeButtonPressed()
    {
        GameManager.Instance.ResumeGame();
    }

    public void OnShowStateButtonPressed()
    {
        Debug.Log("Current Game State: " + GameManager.Instance.GetCurrentState());
    }

    public void OnLoadPokerSceneButtonPressed()
    {
        GameManager.Instance.LoadScene("PokerScene", GameState.PokerPhase);
    }

    public void OnLoadArenaSceneButtonPressed()
    {
        GameManager.Instance.LoadScene("ArenaScene", GameState.ArenaPhase);
    }
}
