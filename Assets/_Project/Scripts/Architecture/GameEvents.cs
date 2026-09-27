using System;
using UnityEngine;

// Patrón Observer: bus de eventos estático. Quien publica no conoce a quien escucha.
// Cada evento indica quién lo dispara (P) y quién lo escucha (E).
public static class GameEvents
{
    // P: PlayerHealth | E: HUDController
    public static event Action<int, int> OnHealthChanged;
    // P: GameManager | E: HUDController
    public static event Action<int, int> OnFruitsChanged;
    // P: PlayerMovement | E: AudioManager, VFXManager, PlayerFeedback
    public static event Action<Vector3> OnPlayerJumped;
    public static event Action<Vector3> OnPlayerLanded;
    public static event Action<Vector3, float> OnPlayerDashed;
    // P: PlayerHealth | E: AudioManager, VFXManager, HUDController, PlayerFeedback, CameraFollow
    public static event Action<Vector3> OnPlayerHurt;
    // P: PlayerHealth | E: GameManager, AudioManager, EndScreenPanel, PlayerMovement
    public static event Action OnPlayerDied;
    // P: Collectible | E: GameManager, AudioManager, VFXManager
    public static event Action<Vector3> OnFruitCollected;
    // P: EnemyPatrol | E: AudioManager, VFXManager
    public static event Action<Vector3> OnEnemyDefeated;
    // P: Goal | E: GameManager
    public static event Action OnGoalReached;
    // P: GameManager | E: AudioManager, EndScreenPanel
    public static event Action OnLevelCompleted;
    // P: GameManager | E: HUDController
    public static event Action<string> OnHint;
    // P: PlayerHealth, PlayerMovement | E: CameraFollow
    public static event Action<float, float> OnCameraShake;
    // P: PlayerInputHandler | E: GameManager
    public static event Action OnPauseRequested;
    public static event Action OnRestartRequested;
    // P: GameManager | E: PauseMenu, AudioManager
    public static event Action<bool> OnPauseChanged;

    public static void HealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
    public static void FruitsChanged(int collected, int total) => OnFruitsChanged?.Invoke(collected, total);
    public static void PlayerJumped(Vector3 pos) => OnPlayerJumped?.Invoke(pos);
    public static void PlayerLanded(Vector3 pos) => OnPlayerLanded?.Invoke(pos);
    public static void PlayerDashed(Vector3 pos, float dir) => OnPlayerDashed?.Invoke(pos, dir);
    public static void PlayerHurt(Vector3 pos) => OnPlayerHurt?.Invoke(pos);
    public static void PlayerDied() => OnPlayerDied?.Invoke();
    public static void FruitCollected(Vector3 pos) => OnFruitCollected?.Invoke(pos);
    public static void EnemyDefeated(Vector3 pos) => OnEnemyDefeated?.Invoke(pos);
    public static void GoalReached() => OnGoalReached?.Invoke();
    public static void LevelCompleted() => OnLevelCompleted?.Invoke();
    public static void Hint(string message) => OnHint?.Invoke(message);
    public static void CameraShake(float duration, float strength) => OnCameraShake?.Invoke(duration, strength);
    public static void PauseRequested() => OnPauseRequested?.Invoke();
    public static void RestartRequested() => OnRestartRequested?.Invoke();
    public static void PauseChanged(bool paused) => OnPauseChanged?.Invoke(paused);
}
