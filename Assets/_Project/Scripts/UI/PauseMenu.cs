using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private void Awake()
    {
        panel.SetActive(false);
        resumeButton.onClick.AddListener(GameEvents.PauseRequested);
        restartButton.onClick.AddListener(GameEvents.RestartRequested);
        menuButton.onClick.AddListener(() => SceneLoader.Load(SceneLoader.MainMenu));
    }

    private void OnEnable() => GameEvents.OnPauseChanged += SetVisible;
    private void OnDisable() => GameEvents.OnPauseChanged -= SetVisible;

    private void SetVisible(bool paused)
    {
        panel.SetActive(paused);
        if (paused) resumeButton.Select();
    }
}
