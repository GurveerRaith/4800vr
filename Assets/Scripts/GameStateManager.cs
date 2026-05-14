using UnityEngine;
using UnityEngine.Events;

// Enum representing the different states of the game.
public enum GameState
{
    GiveMicrophone,
    SitDown,
    WatchingVideo,
    FMBatteryDead,
    TakeQuiz,
    QuizFailed,
    Cutscene,
    EndScreen
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Current State (read-only in Inspector)")]
    [SerializeField] private GameState currentState;

    // Any script can subscribe to this to react to state changes
    public UnityEvent<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetState(GameState.GiveMicrophone);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"[GameState] → {newState}");
        OnStateChanged?.Invoke(newState);
    }

    public GameState GetState() => currentState;
}
