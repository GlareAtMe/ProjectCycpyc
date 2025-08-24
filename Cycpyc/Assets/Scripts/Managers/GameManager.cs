using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PokerPhaseManager pokerPhaseManager;

    public static GameManager Instance { get; private set; }

    /// <summary>Fired after the state actually changes: (previous, current).</summary>
    public event Action<GameState, GameState> OnStateChanged;

    public GameState CurrentState { get; private set; } = GameState.None;
    public GameState PreviousState { get; private set; } = GameState.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        var scene = SceneManager.GetActiveScene().name;

        switch (scene)
        {
            case "PokerScene":
                SetState(GameState.PokerPhase);
                break;

            case "ArenaScene":
                SetState(GameState.ArenaPhase);
                break;

            case "MainMenuScene":
                SetState(GameState.MainMenu);
                break;

            default:
                Debug.LogWarning($"[GameManager] Unknown scene '{scene}', defaulting to MainMenu.");
                SetState(GameState.MainMenu);
                break;
        }
    }

    /// <summary>Safely change game state and notify listeners.</summary>
    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        PreviousState = CurrentState;
        CurrentState = newState;

        Debug.Log($"Game State changed to: {newState}");
        OnStateChanged?.Invoke(PreviousState, CurrentState);
    }

    /// <summary>Entry point for starting the poker phase from UI or flow code.</summary>
    public void InitiatePokerPhase()
    {
        if (pokerPhaseManager == null)
            pokerPhaseManager = FindFirstObjectByType<PokerPhaseManager>();

        SetState(GameState.PokerPhase);
        pokerPhaseManager?.StartPokerPhase();
    }

    public GameState GetCurrentState()
    {
        Debug.Log(CurrentState);
        return CurrentState;
    }

    /// <summary>Pause the game and freeze time.</summary>
    public void PauseGame()
    {
        if (CurrentState == GameState.Paused) return;
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    /// <summary>Resume game and restore previous state.</summary>
    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        var fallback = PreviousState == GameState.None ? GameState.MainMenu : PreviousState;
        SetState(fallback);
    }

    /// <summary>
    /// Load a scene and switch to the provided next state when loading completes.
    /// </summary>
    public void LoadScene(string sceneName, GameState nextState)
    {
        StartCoroutine(LoadSceneAsync(sceneName, nextState));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName, GameState nextState)
    {
        SetState(GameState.Loading);

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!op.isDone) yield return null;

        SetState(nextState);
    }
}
