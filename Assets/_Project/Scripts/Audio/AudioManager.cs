using UnityEngine;
using UnityEngine.Audio;

// Singleton de audio: escucha los eventos del juego y reproduce SFX/BGM a través del AudioMixer.
public class AudioManager : Singleton<AudioManager>
{
    public const string MusicParameter = "MusicVolume";
    public const string SfxParameter = "SfxVolume";
    private const string MusicPref = "volume_music";
    private const string SfxPref = "volume_sfx";

    [Header("Fuentes y mezclador")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioMixer mixer;

    [Header("Música")]
    [SerializeField] private AudioClip music;

    [Header("Efectos")]
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip land;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip hurt;
    [SerializeField] private AudioClip death;
    [SerializeField] private AudioClip fruit;
    [SerializeField] private AudioClip enemyDefeat;
    [SerializeField] private AudioClip levelComplete;
    [SerializeField] private AudioClip pauseIn;
    [SerializeField] private AudioClip pauseOut;
    [SerializeField] private AudioClip goalHint;

    public float MusicVolume { get; private set; } = 0.8f;
    public float SfxVolume { get; private set; } = 1f;

    protected override void Awake()
    {
        base.Awake();
        MusicVolume = PlayerPrefs.GetFloat(MusicPref, MusicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxPref, SfxVolume);
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerJumped += HandleJump;
        GameEvents.OnPlayerLanded += HandleLand;
        GameEvents.OnPlayerDashed += HandleDash;
        GameEvents.OnPlayerHurt += HandleHurt;
        GameEvents.OnPlayerDied += HandleDeath;
        GameEvents.OnFruitCollected += HandleFruit;
        GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
        GameEvents.OnLevelCompleted += HandleLevelCompleted;
        GameEvents.OnPauseChanged += HandlePause;
        GameEvents.OnHint += HandleHint;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerJumped -= HandleJump;
        GameEvents.OnPlayerLanded -= HandleLand;
        GameEvents.OnPlayerDashed -= HandleDash;
        GameEvents.OnPlayerHurt -= HandleHurt;
        GameEvents.OnPlayerDied -= HandleDeath;
        GameEvents.OnFruitCollected -= HandleFruit;
        GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
        GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        GameEvents.OnPauseChanged -= HandlePause;
        GameEvents.OnHint -= HandleHint;
    }

    private void Start()
    {
        SetMusicVolume(MusicVolume);
        SetSfxVolume(SfxVolume);

        if (music != null && musicSource != null)
        {
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void SetMusicVolume(float linear)
    {
        MusicVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(MusicPref, MusicVolume);
        ApplyVolume(MusicParameter, MusicVolume, musicSource);
    }

    public void SetSfxVolume(float linear)
    {
        SfxVolume = Mathf.Clamp01(linear);
        PlayerPrefs.SetFloat(SfxPref, SfxVolume);
        ApplyVolume(SfxParameter, SfxVolume, sfxSource);
    }

    private void ApplyVolume(string parameter, float linear, AudioSource fallback)
    {
        float decibels = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        if (mixer == null || !mixer.SetFloat(parameter, decibels))
            fallback.volume = linear;
    }

    private void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null) sfxSource.PlayOneShot(clip, volume);
    }

    private void HandleJump(Vector3 position) => Play(jump, 0.8f);
    private void HandleLand(Vector3 position) => Play(land, 0.6f);
    private void HandleDash(Vector3 position, float direction) => Play(dash, 0.85f);
    private void HandleHurt(Vector3 position) => Play(hurt);
    private void HandleDeath() => Play(death);
    private void HandleFruit(Vector3 position) => Play(fruit);
    private void HandleEnemyDefeated(Vector3 position) => Play(enemyDefeat);
    private void HandleLevelCompleted() => Play(levelComplete);
    private void HandleHint(string message) => Play(goalHint, 0.7f);
    private void HandlePause(bool paused) => Play(paused ? pauseIn : pauseOut);
}
