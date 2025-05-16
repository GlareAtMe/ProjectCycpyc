using Assets.Scripts.Constants;
using UnityEngine;
using UnityEngine.UI;

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
        GameManager.Instance.LoadScene(SceneConstans.POKER_SCENE, GameState.PokerPhase);
    }

    public void OnLoadArenaSceneButtonPressed()
    {
        GameManager.Instance.LoadScene(SceneConstans.ARENA_SCENE, GameState.ArenaPhase);
    }

    public void OnLoadMainMenuSceneButtonPressed()
    {
        GameManager.Instance.LoadScene(SceneConstans.MAIN_MENU_SCENE, GameState.ArenaPhase);
    }
}
