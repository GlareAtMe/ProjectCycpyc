using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }
    private GameState previousState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetState(GameState.MainMenu);
    }

    public void SetState(GameState newState)
    {
        previousState = CurrentState;
        CurrentState = newState;
        Debug.Log($"Game State changed to: {newState}");
    }

    public GameState GetCurrentState()
    {
        Debug.Log(CurrentState);
        return CurrentState;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SetState(previousState);
    }

    public void LoadScene(string sceneName, GameState nextState)
    {
        SetState(GameState.Loading);
        SceneManager.LoadScene(sceneName);
        SetState(nextState);
    }
}
