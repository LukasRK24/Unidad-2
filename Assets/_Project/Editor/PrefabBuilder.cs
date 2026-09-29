#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Crea los prefabs del juego: jugador, enemigo, fruta, picos y meta.
public static class PrefabBuilder
{
    private const string Out = BuilderUtil.Root + "/Prefabs/";
    private const string Art = BuilderUtil.Root + "/Art/";

    public static readonly string[] FruitNames = { "Apple", "Bananas", "Cherries", "Melon", "Pineapple", "Strawberry", "Orange", "Kiwi" };

    public static PhysicsMaterial2D NoFriction()
    {
        string path = Out + "NoFriction.physicsMaterial2D";
        PhysicsMaterial2D existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (existing != null) return existing;

        PhysicsMaterial2D material = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    public static GameObject BuildPlayer(AnimatorController controller, int groundLayer, int playerLayer)
    {
        GameObject root = new GameObject("Player") { tag = "Player", layer = playerLayer };

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        CapsuleCollider2D body = root.AddComponent<CapsuleCollider2D>();
        body.size = new Vector2(1.1f, 1.5f);
        body.direction = CapsuleDirection2D.Vertical;
        body.sharedMaterial = NoFriction();

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = BuilderUtil.LoadSprite(Art + "Player/NinjaFrog_Idle (32x32).png");
        sr.sortingOrder = 10;
        Animator animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        GameObject groundCheck = new GameObject("GroundCheck");
        groundCheck.transform.SetParent(root.transform);
        groundCheck.transform.localPosition = new Vector3(0f, -0.8f, 0f);

        PlayerMovement movement = root.AddComponent<PlayerMovement>();
        BuilderUtil.SetRef(movement, "groundCheck", groundCheck.transform);
        BuilderUtil.SetInt(movement, "groundMask", 1 << groundLayer);

        root.AddComponent<PlayerInputHandler>();
        root.AddComponent<PlayerHealth>();
        root.AddComponent<PlayerCombat>();

        PlayerAnimator playerAnimator = root.AddComponent<PlayerAnimator>();
        BuilderUtil.SetRef(playerAnimator, "animator", animator);
        BuilderUtil.SetRef(playerAnimator, "spriteRenderer", sr);

        PlayerFeedback feedback = root.AddComponent<PlayerFeedback>();
        BuilderUtil.SetRef(feedback, "visual", visual.transform);
        BuilderUtil.SetRef(feedback, "spriteRenderer", sr);

        return BuilderUtil.SavePrefab(root, Out + "Player.prefab");
    }

    public static GameObject BuildEnemy(int groundLayer, int enemyLayer)
    {
        GameObject root = new GameObject("AngryPig") { layer = enemyLayer };

        Rigidbody2D rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.mass = 2f;

        BoxCollider2D body = root.AddComponent<BoxCollider2D>();
        body.size = new Vector2(1.5f, 1.5f);
        body.sharedMaterial = NoFriction();

        Sprite[] walk = BuilderUtil.LoadSprites(Art + "Enemies/AngryPig_Walk (36x30).png");
        Sprite[] hit = BuilderUtil.LoadSprites(Art + "Enemies/AngryPig_Hit 1 (36x30).png");

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = walk[0];
        sr.sortingOrder = 8;

        SpriteFrameAnimator frames = root.AddComponent<SpriteFrameAnimator>();
        BuilderUtil.SetSprites(frames, "frames", walk);
        BuilderUtil.SetFloat(frames, "framesPerSecond", 14f);

        GameObject wall = new GameObject("WallCheck");
        wall.transform.SetParent(root.transform);
        wall.transform.localPosition = new Vector3(-0.9f, 0f, 0f);
        GameObject ledge = new GameObject("LedgeCheck");
        ledge.transform.SetParent(root.transform);
        ledge.transform.localPosition = new Vector3(-0.9f, -0.6f, 0f);

        EnemyPatrol patrol = root.AddComponent<EnemyPatrol>();
        BuilderUtil.SetRef(patrol, "wallCheck", wall.transform);
        BuilderUtil.SetRef(patrol, "ledgeCheck", ledge.transform);
        BuilderUtil.SetInt(patrol, "groundMask", 1 << groundLayer);
        BuilderUtil.SetRef(patrol, "spriteRenderer", sr);
        BuilderUtil.SetRef(patrol, "frameAnimator", frames);
        BuilderUtil.SetSprites(patrol, "hitFrames", hit);

        return BuilderUtil.SavePrefab(root, Out + "AngryPig.prefab");
    }

    public static GameObject BuildFruit()
    {
        GameObject root = new GameObject("Fruit");

        Sprite[] frames = BuilderUtil.LoadSprites(Art + "Items/Fruit_Apple.png");
        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = frames[0];
        sr.sortingOrder = 6;

        CircleCollider2D trigger = root.AddComponent<CircleCollider2D>();
        trigger.radius = 0.6f;
        trigger.isTrigger = true;

        SpriteFrameAnimator animator = root.AddComponent<SpriteFrameAnimator>();
        BuilderUtil.SetSprites(animator, "frames", frames);
        BuilderUtil.SetFloat(animator, "framesPerSecond", 14f);
        root.AddComponent<Collectible>();

        return BuilderUtil.SavePrefab(root, Out + "Fruit.prefab");
    }

    public static GameObject BuildSpikes()
    {
        GameObject root = new GameObject("Spikes");

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = BuilderUtil.LoadSprite(Art + "Traps/Spikes.png");
        sr.sortingOrder = 4;

        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.size = new Vector2(0.8f, 0.45f);
        trigger.offset = new Vector2(0f, -0.2f);
        trigger.isTrigger = true;

        root.AddComponent<Hazard>();
        return BuilderUtil.SavePrefab(root, Out + "Spikes.prefab");
    }

    public static GameObject BuildGoal()
    {
        GameObject root = new GameObject("Goal");

        SpriteRenderer sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = BuilderUtil.LoadSprite(Art + "Items/Goal_Idle.png");
        sr.sortingOrder = 3;

        BoxCollider2D trigger = root.AddComponent<BoxCollider2D>();
        trigger.size = new Vector2(2f, 3.5f);
        trigger.isTrigger = true;

        root.AddComponent<Goal>();
        return BuilderUtil.SavePrefab(root, Out + "Goal.prefab");
    }
}
#endif
