#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Construye el nivel con Tilemap + Composite Collider 2D y coloca enemigos, trampas, frutas y meta.
public static class LevelBuilder
{
    public static readonly Vector2 LevelMin = new Vector2(-1f, -5f);
    public static readonly Vector2 LevelMax = new Vector2(98f, 22f);

    private static Tilemap tilemap;
    private static Tile[,] grass;

    public static Tilemap BuildTilemap(Transform parent, int groundLayer)
    {
        grass = CreateTiles();

        GameObject gridObject = new GameObject("Grid");
        gridObject.transform.SetParent(parent);
        gridObject.AddComponent<Grid>();

        GameObject groundObject = new GameObject("Ground") { layer = groundLayer };
        groundObject.transform.SetParent(gridObject.transform);
        tilemap = groundObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = groundObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 1;

        Rigidbody2D rb = groundObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        TilemapCollider2D tileCollider = groundObject.AddComponent<TilemapCollider2D>();
        CompositeCollider2D composite = groundObject.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        groundObject.AddComponent<TilemapCompositeInitializer>();

        Paint();
        tilemap.CompressBounds();
        return tilemap;
    }

    private static Tile[,] CreateTiles()
    {
        string sheet = BuilderUtil.Root + "/Art/Terrain/Terrain.png";
        BuilderUtil.SetPixelsPerUnit(sheet, 16f);
        string folder = BuilderUtil.Root + "/Art/Terrain/Tiles";
        BuilderUtil.EnsureFolder(folder);

        int[,] ids = { { 5, 6, 7 }, { 22, 23, 24 }, { 37, 38, 39 } };
        Tile[,] tiles = new Tile[3, 3];
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = BuilderUtil.LoadSpriteByName(sheet, "Terrain (16x16) 1_" + ids[r, c]);
                tile.colliderType = Tile.ColliderType.Grid;
                string path = $"{folder}/Grass_{r}{c}.asset";
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(tile, path);
                tiles[r, c] = tile;
            }
        }
        return tiles;
    }

    // Mapa del nivel: suelo con "yTop" como fila de la superficie.
    private static void Paint()
    {
        Fill(-3, -1, 20, -4);       // pared izquierda
        Fill(0, 20, 0, -4);         // sección 1: tutorial
        Fill(25, 38, 0, -4);        // sección 2: picos y plataforma
        Fill(44, 52, 0, -4);        // sección 3
        Fill(53, 66, 2, -4);        // escalón
        Fill(83, 97, 8, 4);         // meseta final
        Fill(98, 100, 20, -4);      // pared derecha

        Platform(28, 31, 3);
        Platform(68, 71, 4);
        Platform(73, 76, 6);
        Platform(78, 81, 8);
    }

    private static void Fill(int x0, int x1, int yTop, int yBottom)
    {
        for (int x = x0; x <= x1; x++)
        {
            for (int y = yBottom; y <= yTop; y++)
            {
                bool leftEmpty = !IsSolid(x - 1, y, x0, x1, yTop, yBottom);
                bool rightEmpty = !IsSolid(x + 1, y, x0, x1, yTop, yBottom);
                int col = leftEmpty ? 0 : rightEmpty ? 2 : 1;
                int row = y == yTop ? 0 : y == yBottom ? 2 : 1;
                tilemap.SetTile(new Vector3Int(x, y, 0), grass[row, col]);
            }
        }
    }

    private static bool IsSolid(int x, int y, int x0, int x1, int yTop, int yBottom)
    {
        return x >= x0 && x <= x1 && y >= yBottom && y <= yTop;
    }

    private static void Platform(int x0, int x1, int y)
    {
        for (int x = x0; x <= x1; x++)
        {
            int col = x == x0 ? 0 : x == x1 ? 2 : 1;
            tilemap.SetTile(new Vector3Int(x, y, 0), grass[0, col]);
        }
    }

    public static void PlaceEntities(Transform parent, GameObject player, GameObject enemy, GameObject fruit, GameObject spikes, GameObject goal, int groundLayer)
    {
        Spawn(player, new Vector3(4f, 1.8f, 0f), parent);

        Spawn(enemy, new Vector3(16f, 1.8f, 0f), parent);
        Spawn(enemy, new Vector3(60f, 3.8f, 0f), parent);
        Spawn(enemy, new Vector3(91f, 9.8f, 0f), parent);

        float[][] fruits =
        {
            new[] { 7f, 2.6f }, new[] { 10f, 2.6f }, new[] { 13f, 2.6f },
            new[] { 29.5f, 5.6f }, new[] { 47f, 2.6f }, new[] { 59f, 4.6f },
            new[] { 70f, 6.6f }, new[] { 75f, 8.6f }, new[] { 92f, 10.6f }
        };
        for (int i = 0; i < fruits.Length; i++)
        {
            GameObject f = Spawn(fruit, new Vector3(fruits[i][0], fruits[i][1], 0f), parent);
            string name = PrefabBuilder.FruitNames[i % PrefabBuilder.FruitNames.Length];
            Sprite[] frames = BuilderUtil.LoadSprites(BuilderUtil.Root + "/Art/Items/Fruit_" + name + ".png");
            BuilderUtil.SetSprites(f.GetComponent<SpriteFrameAnimator>(), "frames", frames);
        }

        float[] spikeX = { 33.5f, 34.5f, 35.5f, 87.5f, 88.5f };
        float[] spikeY = { 1.5f, 1.5f, 1.5f, 9.5f, 9.5f };
        for (int i = 0; i < spikeX.Length; i++) Spawn(spikes, new Vector3(spikeX[i], spikeY[i], 0f), parent);

        Spawn(goal, new Vector3(95f, 11f, 0f), parent);

        GameObject kill = new GameObject("KillZone") { layer = groundLayer };
        kill.transform.SetParent(parent);
        kill.transform.position = new Vector3(48f, -9f, 0f);
        BoxCollider2D box = kill.AddComponent<BoxCollider2D>();
        box.size = new Vector2(220f, 4f);
        box.isTrigger = true;
        kill.layer = 0;
        BuilderUtil.SetInt(kill.AddComponent<Hazard>(), "damage", 999);
        SerializedObject so = new SerializedObject(kill.GetComponent<Hazard>());
        so.FindProperty("instantKill").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject Spawn(GameObject prefab, Vector3 position, Transform parent)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        return instance;
    }
}
#endif
