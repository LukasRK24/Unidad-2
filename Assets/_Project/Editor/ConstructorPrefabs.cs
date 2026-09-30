#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Crea los prefabs del juego: jugador, enemigo, fruta, picos y meta.
public static class ConstructorPrefabs
{
    private const string Out = UtilConstructor.Raiz + "/Prefabs/";
    private const string Art = UtilConstructor.Raiz + "/Art/";

    public static readonly string[] NombresFrutas = { "Apple", "Bananas", "Cherries", "Melon", "Pineapple", "Strawberry", "Orange", "Kiwi" };

    public static PhysicsMaterial2D SinFriccion()
    {
        string path = Out + "SinFriccion.physicsMaterial2D";
        PhysicsMaterial2D existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (existing != null) return existing;

        PhysicsMaterial2D material = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    public static GameObject ConstruirJugador(AnimatorController controller, int groundLayer, int playerLayer)
    {
        GameObject root = new GameObject("Jugador") { tag = "Player", layer = playerLayer };

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        CapsuleCollider2D body = root.AddComponent<CapsuleCollider2D>();
        body.size = new Vector2(1.1f, 1.5f);
        body.direction = CapsuleDirection2D.Vertical;
        body.sharedMaterial = SinFriccion();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = UtilConstructor.CargarSprite(Art + "Player/NinjaFrog_Idle (32x32).png");
        sr.sortingOrder = 10;
        Animator animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        GameObject groundCheck = new GameObject("DetectorSuelo");
        groundCheck.transform.SetParent(root.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.8f, 0f);

        MovimientoJugador movement = root.AddComponent<MovimientoJugador>();
        UtilConstructor.AsignarReferencia(movement, "detectorSuelo", groundCheck.transform);
        UtilConstructor.AsignarEntero(movement, "capaSuelo", 1 << groundLayer);

        root.AddComponent<ControladorEntradaJugador>();
        root.AddComponent<VidaJugador>();
        root.AddComponent<CombateJugador>();

        AnimadorJugador playerAnimator = root.AddComponent<AnimadorJugador>();
        UtilConstructor.AsignarReferencia(playerAnimator, "animador", animator);
        UtilConstructor.AsignarReferencia(playerAnimator, "renderizador", sr);

        EfectosJugador feedback = root.AddComponent<EfectosJugador>();
        UtilConstructor.AsignarReferencia(feedback, "modeloVisual", visual.transform);
        UtilConstructor.AsignarReferencia(feedback, "renderizador", sr);

        return UtilConstructor.GuardarPrefab(root, Out + "Jugador.prefab");
    }

    public static GameObject ConstruirEnemigo(int groundLayer, int enemyLayer)
    {
        GameObject root = new GameObject("AngryPig") { layer = enemyLayer };

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.mass = 2f;

        BoxCollider2D body = root.AddComponent<BoxCollider2D>();
        body.size = new Vector2(1.5f, 1.5f);
        body.sharedMaterial = SinFriccion();

        Sprite[] walk = UtilConstructor.CargarSprites(Art + "Enemies/AngryPig_Walk (36x30).png");
        Sprite[] hit = UtilConstructor.CargarSprites(Art + "Enemies/AngryPig_Hit 1 (36x30).png");

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = walk[0];
        sr.sortingOrder = 8;

        AnimadorSprites frames = root.AddComponent<AnimadorSprites>();
        UtilConstructor.AsignarSprites(frames, "sprites", walk);
        UtilConstructor.AsignarDecimal(frames, "fotogramasPorSegundo", 14f);

        GameObject wall = new GameObject("DetectorPared");
        wall.transform.SetParent(root.transform);
        wall.transform.localPosition = new Vector3(-0.9f, 0f, 0f);
        GameObject ledge = new GameObject("DetectorBorde");
        ledge.transform.SetParent(root.transform);
        ledge.transform.localPosition = new Vector3(-0.9f, -0.6f, 0f);

        EnemigoPatrulla patrol = root.AddComponent<EnemigoPatrulla>();
        UtilConstructor.AsignarReferencia(patrol, "detectorPared", wall.transform);
        UtilConstructor.AsignarReferencia(patrol, "detectorBorde", ledge.transform);
        UtilConstructor.AsignarEntero(patrol, "capaSuelo", 1 << groundLayer);
        UtilConstructor.AsignarReferencia(patrol, "renderizador", sr);
        UtilConstructor.AsignarReferencia(patrol, "animadorSprites", frames);
        UtilConstructor.AsignarSprites(patrol, "spritesGolpe", hit);

        return UtilConstructor.GuardarPrefab(root, Out + "AngryPig.prefab");
    }

    public static GameObject ConstruirFruta()
    {
        GameObject root = new GameObject("Fruta");

        Sprite[] frames = UtilConstructor.CargarSprites(Art + "Items/Fruit_Apple.png");
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = frames[0];
        sr.sortingOrder = 6;

        CircleCollider2D trigger = root.AddComponent<CircleCollider2D>();
        trigger.radius = 0.6f;
        trigger.isTrigger = true;

        AnimadorSprites animator = root.AddComponent<AnimadorSprites>();
        UtilConstructor.AsignarSprites(animator, "sprites", frames);
        UtilConstructor.AsignarDecimal(animator, "fotogramasPorSegundo", 14f);
        root.AddComponent<Coleccionable>();

        return UtilConstructor.GuardarPrefab(root, Out + "Fruta.prefab");
    }

    public static GameObject ConstruirPicos()
    {
        GameObject root = new GameObject("Picos");

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = UtilConstructor.CargarSprite(Art + "Traps/Spikes.png");
        sr.sortingOrder = 4;

        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.size = new Vector2(0.8f, 0.45f);
        trigger.offset = new Vector2(0f, -0.2f);
        trigger.isTrigger = true;

        root.AddComponent<Peligro>();
        return UtilConstructor.GuardarPrefab(root, Out + "Picos.prefab");
    }

    public static GameObject ConstruirMeta()
    {
        GameObject root = new GameObject("Meta");

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = UtilConstructor.CargarSprite(Art + "Items/Goal_Idle.png");
        sr.sortingOrder = 3;

        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.size = new Vector2(2f, 3.5f);
        trigger.isTrigger = true;

        root.AddComponent<Meta>();
        return UtilConstructor.GuardarPrefab(root, Out + "Meta.prefab");
    }
}
#endif
