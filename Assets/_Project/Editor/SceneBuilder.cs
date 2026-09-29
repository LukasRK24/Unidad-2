#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneBuilder
{
    private const string Scenes = BuilderUtil.Root + "/Scenes/";
    private const string Audio = BuilderUtil.Root + "/Audio/";

    public static void BuildLevel(AnimatorController controller, AudioMixer mixer, ParticleSystem[] vfx, int groundLayer, int enemyLayer, int playerLayer)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateGlobalLight();

        GameObject world = new GameObject("World");
        LevelBuilder.BuildTilemap(world.transform, groundLayer);

        GameObject player = PrefabBuilder.BuildPlayer(controller, groundLayer, playerLayer);
        GameObject enemy = PrefabBuilder.BuildEnemy(groundLayer, enemyLayer);
        GameObject fruit = PrefabBuilder.BuildFruit();
        GameObject spikes = PrefabBuilder.BuildSpikes();
        GameObject goal = PrefabBuilder.BuildGoal();

        GameObject entities = new GameObject("Entities");
        LevelBuilder.PlaceEntities(entities.transform, player, enemy, fruit, spikes, goal, groundLayer);
        GameObject playerInstance = GameObject.FindGameObjectWithTag("Player");

        BuildCamera(playerInstance.transform);
        BuildManagers(mixer, vfx);
        BuildHud();
        UiBuilder.CreateEventSystem();

        EditorSceneManager.SaveScene(scene, Scenes + "Level_01.unity");
    }

    public static void BuildMainMenu(AudioMixer mixer)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camObject = new GameObject("Main Camera") { tag = "MainCamera" };
        Camera cam = camObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.backgroundColor = new Color(0.45f, 0.7f, 0.95f);
        camObject.AddComponent<AudioListener>();

        GameObject managers = new GameObject("Managers");
        AddAudioManager(managers, mixer);

        Canvas canvas = UiBuilder.CreateCanvas("Canvas");
        Image background = UiBuilder.Image("Background", canvas.transform, Color.white, BuilderUtil.LoadSprite(BuilderUtil.Root + "/Art/Background/Mountains.png"));
        UiBuilder.Anchor(background.rectTransform, Vector2.zero, Vector2.one);
        background.preserveAspect = false;

        Backdrop(canvas.transform, "TitleBackdrop", new Vector2(0f, 285f), new Vector2(1150f, 250f));
        Backdrop(canvas.transform, "SlidersBackdrop", new Vector2(0f, -265f), new Vector2(820f, 190f));
        Backdrop(canvas.transform, "ControlsBackdrop", new Vector2(0f, -440f), new Vector2(1560f, 70f));

        TextMeshProUGUI title = UiBuilder.Text("Title", canvas.transform, "UNIDAD 2", 130f, TextAlignmentOptions.Center, Color.white);
        title.rectTransform.anchoredPosition = new Vector2(0f, 330f);
        title.rectTransform.sizeDelta = new Vector2(1200f, 160f);
        title.fontStyle = FontStyles.Bold;
        TextMeshProUGUI subtitle = UiBuilder.Text("Subtitle", canvas.transform, "Arquitectura de Sistemas Integrados", 44f, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.9f));
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, 230f);
        subtitle.rectTransform.sizeDelta = new Vector2(1200f, 70f);

        Button play = UiBuilder.Button("PlayButton", canvas.transform, "JUGAR", new Vector2(0f, 80f), new Vector2(520f, 110f));
        Button quit = UiBuilder.Button("QuitButton", canvas.transform, "SALIR", new Vector2(0f, -60f), new Vector2(520f, 110f));

        TextMeshProUGUI musicLabel = UiBuilder.Text("MusicLabel", canvas.transform, "Música", 36f, TextAlignmentOptions.Right, Color.white);
        musicLabel.rectTransform.anchoredPosition = new Vector2(-330f, -220f);
        musicLabel.rectTransform.sizeDelta = new Vector2(240f, 60f);
        Slider music = UiBuilder.Slider("MusicSlider", canvas.transform, new Vector2(40f, -220f), new Vector2(420f, 50f));
        TextMeshProUGUI sfxLabel = UiBuilder.Text("SfxLabel", canvas.transform, "Efectos", 36f, TextAlignmentOptions.Right, Color.white);
        sfxLabel.rectTransform.anchoredPosition = new Vector2(-330f, -310f);
        sfxLabel.rectTransform.sizeDelta = new Vector2(240f, 60f);
        Slider sfx = UiBuilder.Slider("SfxSlider", canvas.transform, new Vector2(40f, -310f), new Vector2(420f, 50f));

        TextMeshProUGUI controls = UiBuilder.Text("Controls", canvas.transform,
            "A/D mover   Espacio saltar   Shift dash (derrota enemigos)   Esc pausa   R reiniciar", 30f, TextAlignmentOptions.Center, Color.white);
        controls.rectTransform.anchoredPosition = new Vector2(0f, -440f);
        controls.rectTransform.sizeDelta = new Vector2(1500f, 60f);

        MainMenu menu = canvas.gameObject.AddComponent<MainMenu>();
        BuilderUtil.SetRef(menu, "playButton", play);
        BuilderUtil.SetRef(menu, "quitButton", quit);
        BuilderUtil.SetRef(menu, "musicSlider", music);
        BuilderUtil.SetRef(menu, "sfxSlider", sfx);

        UiBuilder.CreateEventSystem();
        EditorSceneManager.SaveScene(scene, Scenes + "MainMenu.unity");
    }

    private static void Backdrop(Transform parent, string name, Vector2 position, Vector2 size)
    {
        Image image = UiBuilder.Image(name, parent, new Color(0.05f, 0.08f, 0.14f, 0.55f), UiBuilder.BoxSprite);
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;
        image.raycastTarget = false;
    }

    private static void CreateGlobalLight()
    {
        GameObject go = new GameObject("Global Light 2D");
        Light2D light = go.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
    }

    private static void BuildCamera(Transform player)
    {
        GameObject camObject = new GameObject("Main Camera") { tag = "MainCamera" };
        Camera cam = camObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.backgroundColor = new Color(0.45f, 0.7f, 0.95f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camObject.AddComponent<AudioListener>();
        camObject.transform.position = new Vector3(0f, 3f, -10f);

        CameraFollow follow = camObject.AddComponent<CameraFollow>();
        BuilderUtil.SetRef(follow, "target", player);
        BuilderUtil.SetVector2(follow, "levelMin", LevelBuilder.LevelMin);
        BuilderUtil.SetVector2(follow, "levelMax", LevelBuilder.LevelMax);

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(camObject.transform, false);
        bg.transform.localPosition = new Vector3(0f, 0f, 20f);
        bg.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = BuilderUtil.LoadSprite(BuilderUtil.Root + "/Art/Background/Mountains.png");
        sr.sortingOrder = -50;
    }

    private static void BuildManagers(AudioMixer mixer, ParticleSystem[] vfx)
    {
        GameObject managers = new GameObject("Managers");
        managers.AddComponent<GameManager>();
        AddAudioManager(managers, mixer);

        GameObject vfxObject = new GameObject("VFXManager");
        vfxObject.transform.SetParent(managers.transform);
        VFXManager manager = vfxObject.AddComponent<VFXManager>();
        BuilderUtil.SetRef(manager, "dustPrefab", vfx[0]);
        BuilderUtil.SetRef(manager, "dashPrefab", vfx[1]);
        BuilderUtil.SetRef(manager, "sparklePrefab", vfx[2]);
        BuilderUtil.SetRef(manager, "hitPrefab", vfx[3]);
    }

    private static void AddAudioManager(GameObject parent, AudioMixer mixer)
    {
        GameObject audioObject = new GameObject("AudioManager");
        audioObject.transform.SetParent(parent.transform);
        AudioManager manager = audioObject.AddComponent<AudioManager>();

        AudioSource music = new GameObject("MusicSource").AddComponent<AudioSource>();
        music.transform.SetParent(audioObject.transform);
        music.playOnAwake = false;
        AudioSource sfx = new GameObject("SfxSource").AddComponent<AudioSource>();
        sfx.transform.SetParent(audioObject.transform);
        sfx.playOnAwake = false;

        if (mixer != null)
        {
            music.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
            sfx.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
        }

        BuilderUtil.SetRef(manager, "musicSource", music);
        BuilderUtil.SetRef(manager, "sfxSource", sfx);
        BuilderUtil.SetRef(manager, "mixer", mixer);
        BuilderUtil.SetRef(manager, "music", Clip("Music/bgm_main.wav"));
        BuilderUtil.SetRef(manager, "jump", Clip("SFX/jump.wav"));
        BuilderUtil.SetRef(manager, "land", Clip("SFX/land.wav"));
        BuilderUtil.SetRef(manager, "dash", Clip("SFX/dash.wav"));
        BuilderUtil.SetRef(manager, "hurt", Clip("SFX/player_hurt.wav"));
        BuilderUtil.SetRef(manager, "death", Clip("SFX/player_death.wav"));
        BuilderUtil.SetRef(manager, "fruit", Clip("SFX/fruit.wav"));
        BuilderUtil.SetRef(manager, "enemyDefeat", Clip("SFX/enemy_defeat.wav"));
        BuilderUtil.SetRef(manager, "levelComplete", Clip("SFX/level_complete.wav"));
        BuilderUtil.SetRef(manager, "pauseIn", Clip("SFX/pause_in.wav"));
        BuilderUtil.SetRef(manager, "pauseOut", Clip("SFX/pause_out.wav"));
        BuilderUtil.SetRef(manager, "goalHint", Clip("SFX/goal_hint.wav"));
    }

    private static AudioClip Clip(string relative) => AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + relative);

    private static void BuildHud()
    {
        Canvas canvas = UiBuilder.CreateCanvas("HUD Canvas");
        Transform root = canvas.transform;

        // Barra de vida (arriba a la izquierda)
        Vector2 topLeft = new Vector2(0f, 1f);
        RectTransform heartRect = UiBuilder.Rect("HeartIcon", root, topLeft, topLeft, topLeft, new Vector2(40f, -35f), new Vector2(70f, 70f));
        Image heart = heartRect.gameObject.AddComponent<Image>();
        heart.sprite = BuilderUtil.LoadSprite(BuilderUtil.Root + "/Art/UI/Heart.png");
        heart.raycastTarget = false;

        RectTransform barBg = UiBuilder.Rect("HealthBarBackground", root, topLeft, topLeft, topLeft, new Vector2(125f, -45f), new Vector2(420f, 50f));
        Image bg = barBg.gameObject.AddComponent<Image>();
        bg.sprite = UiBuilder.BoxSprite;
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
        bg.raycastTarget = false;

        Image fill = UiBuilder.Image("HealthFill", barBg, new Color(0.9f, 0.2f, 0.25f), UiBuilder.BoxSprite);
        UiBuilder.Anchor(fill.rectTransform, Vector2.zero, Vector2.one);
        fill.rectTransform.offsetMin = new Vector2(4f, 4f);
        fill.rectTransform.offsetMax = new Vector2(-4f, -4f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        fill.raycastTarget = false;

        TextMeshProUGUI healthLabel = UiBuilder.Text("HealthLabel", barBg, "3/3", 32f, TextAlignmentOptions.Center, Color.white);
        UiBuilder.Anchor(healthLabel.rectTransform, Vector2.zero, Vector2.one);

        // Frutas (arriba a la derecha)
        Vector2 topRight = new Vector2(1f, 1f);
        RectTransform fruitBg = UiBuilder.Rect("FruitBackground", root, topRight, topRight, topRight, new Vector2(-30f, -30f), new Vector2(330f, 80f));
        Image fruitBgImage = fruitBg.gameObject.AddComponent<Image>();
        fruitBgImage.sprite = UiBuilder.BoxSprite;
        fruitBgImage.type = Image.Type.Sliced;
        fruitBgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);
        fruitBgImage.raycastTarget = false;
        RectTransform fruitRect = UiBuilder.Rect("FruitIcon", root, topRight, topRight, topRight, new Vector2(-260f, -25f), new Vector2(80f, 80f));
        Image fruitIcon = fruitRect.gameObject.AddComponent<Image>();
        fruitIcon.sprite = BuilderUtil.LoadSprite(BuilderUtil.Root + "/Art/Items/Fruit_Apple.png");
        fruitIcon.raycastTarget = false;
        TextMeshProUGUI fruitsLabel = UiBuilder.Text("FruitsLabel", root, "0 / 0", 54f, TextAlignmentOptions.Left, Color.white);
        fruitsLabel.rectTransform.anchorMin = topRight;
        fruitsLabel.rectTransform.anchorMax = topRight;
        fruitsLabel.rectTransform.pivot = topRight;
        fruitsLabel.rectTransform.anchoredPosition = new Vector2(-40f, -35f);
        fruitsLabel.rectTransform.sizeDelta = new Vector2(220f, 70f);

        // Mensaje (abajo al centro) y controles
        TextMeshProUGUI hint = UiBuilder.Text("HintLabel", root, string.Empty, 48f, TextAlignmentOptions.Center, new Color(1f, 0.93f, 0.4f));
        hint.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        hint.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        hint.rectTransform.pivot = new Vector2(0.5f, 0f);
        hint.rectTransform.anchoredPosition = new Vector2(0f, 120f);
        hint.rectTransform.sizeDelta = new Vector2(1400f, 80f);

        RectTransform controlsBg = UiBuilder.Rect("ControlsBackground", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1280f, 60f));
        Image controlsBgImage = controlsBg.gameObject.AddComponent<Image>();
        controlsBgImage.sprite = UiBuilder.BoxSprite;
        controlsBgImage.type = Image.Type.Sliced;
        controlsBgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.6f);
        controlsBgImage.raycastTarget = false;

        TextMeshProUGUI controls = UiBuilder.Text("ControlsLabel", root, "A/D mover  |  Espacio saltar  |  Shift dash  |  Esc pausa  |  R reiniciar", 26f, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.8f));
        controls.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        controls.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        controls.rectTransform.pivot = new Vector2(0.5f, 0f);
        controls.rectTransform.anchoredPosition = new Vector2(0f, 30f);
        controls.rectTransform.sizeDelta = new Vector2(1500f, 50f);

        // Flash rojo de daño
        Image overlay = UiBuilder.Image("DamageOverlay", root, new Color(1f, 0f, 0f, 0f));
        UiBuilder.Anchor(overlay.rectTransform, Vector2.zero, Vector2.one);
        overlay.raycastTarget = false;

        HUDController hud = canvas.gameObject.AddComponent<HUDController>();
        BuilderUtil.SetRef(hud, "healthFill", fill);
        BuilderUtil.SetRef(hud, "healthLabel", healthLabel);
        BuilderUtil.SetRef(hud, "damageOverlay", overlay);
        BuilderUtil.SetRef(hud, "fruitsLabel", fruitsLabel);
        BuilderUtil.SetRef(hud, "hintLabel", hint);

        BuildPause(root);
        BuildEndPanel(root, EndScreenPanel.Mode.Victory, "¡NIVEL COMPLETADO!", new Color(0.4f, 1f, 0.5f));
        BuildEndPanel(root, EndScreenPanel.Mode.GameOver, "GAME OVER", new Color(1f, 0.4f, 0.4f));
    }

    private static void BuildPause(Transform root)
    {
        GameObject panel = UiBuilder.Panel("PausePanel", root, "PAUSA", out TextMeshProUGUI title, "Continuar", "Reiniciar", "Menú principal");
        Transform box = panel.transform.Find("Box");
        Button resume = UiBuilder.Button("ResumeButton", box, "CONTINUAR", new Vector2(0f, 20f), new Vector2(520f, 100f));
        Button restart = UiBuilder.Button("RestartButton", box, "REINICIAR", new Vector2(0f, -100f), new Vector2(520f, 100f));
        Button menu = UiBuilder.Button("MenuButton", box, "MENÚ PRINCIPAL", new Vector2(0f, -220f), new Vector2(520f, 100f));

        GameObject holder = new GameObject("PauseMenu");
        holder.transform.SetParent(root, false);
        PauseMenu pause = holder.AddComponent<PauseMenu>();
        BuilderUtil.SetRef(pause, "panel", panel);
        BuilderUtil.SetRef(pause, "resumeButton", resume);
        BuilderUtil.SetRef(pause, "restartButton", restart);
        BuilderUtil.SetRef(pause, "menuButton", menu);
    }

    private static void BuildEndPanel(Transform root, EndScreenPanel.Mode mode, string titleText, Color titleColor)
    {
        GameObject panel = UiBuilder.Panel(mode + "Panel", root, titleText, out TextMeshProUGUI title, "Reiniciar", "Menú");
        title.color = titleColor;
        Transform box = panel.transform.Find("Box");
        Button restart = UiBuilder.Button("RestartButton", box, "JUGAR DE NUEVO", new Vector2(0f, -10f), new Vector2(520f, 100f));
        Button menu = UiBuilder.Button("MenuButton", box, "MENÚ PRINCIPAL", new Vector2(0f, -130f), new Vector2(520f, 100f));

        GameObject holder = new GameObject(mode + "Screen");
        holder.transform.SetParent(root, false);
        EndScreenPanel script = holder.AddComponent<EndScreenPanel>();
        BuilderUtil.SetInt(script, "mode", (int)mode);
        BuilderUtil.SetRef(script, "panel", panel);
        BuilderUtil.SetRef(script, "title", title);
        BuilderUtil.SetRef(script, "restartButton", restart);
        BuilderUtil.SetRef(script, "menuButton", menu);
    }
}
#endif
