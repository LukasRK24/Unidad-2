#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Crea los prefabs de partículas (polvo, dash, destello de fruta y golpe) con ParticleSystem.
public static class VfxBuilder
{
    private const string Out = BuilderUtil.Root + "/Prefabs/VFX/";

    public static ParticleSystem[] Build()
    {
        BuilderUtil.EnsureFolder(Out);
        BuilderUtil.SetPixelsPerUnit(BuilderUtil.Root + "/Art/Effects/DustParticle.png", 16f);

        Material material = new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(BuilderUtil.Root + "/Art/Effects/DustParticle.png");
        AssetDatabase.DeleteAsset(Out + "VFX_Particle.mat");
        AssetDatabase.CreateAsset(material, Out + "VFX_Particle.mat");

        ParticleSystem dust = Make("VFX_Dust", material, 7, 0.35f, new Vector2(0.8f, 2f), new Vector2(0.25f, 0.5f),
            new Color(0.9f, 0.85f, 0.75f, 0.9f), 25f, -0.3f, true);
        ParticleSystem dash = Make("VFX_Dash", material, 12, 0.3f, new Vector2(1f, 4f), new Vector2(0.25f, 0.55f),
            new Color(0.7f, 0.95f, 1f, 0.9f), 20f, 0f, true);
        ParticleSystem sparkle = Make("VFX_Sparkle", material, 14, 0.6f, new Vector2(3f, 6f), new Vector2(0.2f, 0.4f),
            new Color(1f, 0.9f, 0.3f, 1f), 360f, 0f, false);
        ParticleSystem hit = Make("VFX_Hit", material, 16, 0.5f, new Vector2(4f, 8f), new Vector2(0.25f, 0.5f),
            new Color(1f, 0.35f, 0.25f, 1f), 360f, 0f, false);

        return new[] { dust, dash, sparkle, hit };
    }

    private static ParticleSystem Make(string name, Material material, int count, float lifetime, Vector2 speed,
        Vector2 size, Color color, float angle, float gravity, bool upward)
    {
        GameObject go = new GameObject(name);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.startColor = color;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 64;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = ps.shape;
        if (upward)
        {
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = Mathf.Min(angle, 60f);
            shape.radius = 0.2f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
        }
        else
        {
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;
        }

        ParticleSystem.ColorOverLifetimeModule colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLife.color = fade;

        ParticleSystem.SizeOverLifetimeModule sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.sortingOrder = 20;

        GameObject prefab = BuilderUtil.SavePrefab(go, Out + name + ".prefab");
        return prefab.GetComponent<ParticleSystem>();
    }
}
#endif
