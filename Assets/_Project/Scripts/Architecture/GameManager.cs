using UnityEngine;

// Singleton: solo gestiona el estado de la partida (frutas, pausa y fin de nivel).
public class GameManager : Singleton<GameManager>
{
    public int FruitsCollected { get; private set; }
    public int FruitsTotal { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsFinished { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        GameEvents.OnFruitCollected += HandleFruitCollected;
        GameEvents.OnGoalReached += HandleGoalReached;
        GameEvents.OnPlayerDied += HandlePlayerDied;
        GameEvents.OnPauseRequested += TogglePause;
        GameEvents.OnRestartRequested += SceneLoader.ReloadCurrent;
    }

    private void OnDisable()
    {
        GameEvents.OnFruitCollected -= HandleFruitCollected;
        GameEvents.OnGoalReached -= HandleGoalReached;
        GameEvents.OnPlayerDied -= HandlePlayerDied;
        GameEvents.OnPauseRequested -= TogglePause;
        GameEvents.OnRestartRequested -= SceneLoader.ReloadCurrent;
    }

    protected override void OnDestroy()
    {
        Time.timeScale = 1f;
        base.OnDestroy();
    }

    private void Start()
    {
        FruitsTotal = FindObjectsByType<Collectible>().Length;
        GameEvents.FruitsChanged(FruitsCollected, FruitsTotal);
    }

    private void HandleFruitCollected(Vector3 position)
    {
        FruitsCollected++;
        GameEvents.FruitsChanged(FruitsCollected, FruitsTotal);
    }

    private void HandleGoalReached()
    {
        if (IsFinished) return;

        if (FruitsCollected < FruitsTotal)
        {
            GameEvents.Hint($"Te faltan {FruitsTotal - FruitsCollected} frutas para terminar");
            return;
        }

        IsFinished = true;
        GameEvents.LevelCompleted();
    }

    private void HandlePlayerDied()
    {
        IsFinished = true;
    }

    public void TogglePause()
    {
        if (IsFinished) return;
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        GameEvents.PauseChanged(IsPaused);
    }
}
