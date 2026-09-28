using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        playButton.onClick.AddListener(() => SceneLoader.Load(SceneLoader.Level01));
        quitButton.onClick.AddListener(SceneLoader.Quit);

        musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);

        playButton.Select();
    }
}
