using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de fin de partida reutilizable: se usa para Victoria y para Game Over según el evento.
public class EndScreenPanel : MonoBehaviour
{
    public enum Mode { Victory, GameOver }

    [SerializeField] private Mode mode;
    [SerializeField] private float showDelay = 0.8f;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text title;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private void Awake()
    {
        panel.SetActive(false);
        restartButton.onClick.AddListener(GameEvents.RestartRequested);
        menuButton.onClick.AddListener(() => SceneLoader.Load(SceneLoader.MainMenu));
    }

    private void OnEnable()
    {
        if (mode == Mode.Victory) GameEvents.OnLevelCompleted += Show;
        else GameEvents.OnPlayerDied += Show;
    }

    private void OnDisable()
    {
        if (mode == Mode.Victory) GameEvents.OnLevelCompleted -= Show;
        else GameEvents.OnPlayerDied -= Show;
    }

    private void Show() => Invoke(nameof(Open), showDelay);

    private void Open()
    {
        panel.SetActive(true);
        restartButton.Select();
    }
}
