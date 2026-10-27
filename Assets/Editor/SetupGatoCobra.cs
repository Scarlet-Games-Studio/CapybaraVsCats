#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupGatoCobra
{
    const string SheetPath = "Assets/inimigos/GatoCobra/GatoCobra-Sheet.png";
    const string PrefabPath = "Assets/Resources/Enemies/GatoCobra.prefab";
    const string ScenePath = "Assets/Scenes/ingame.unity";
    const string ProjectilePath = "Assets/inimigos/MinionProjectile 1.prefab";

    static readonly (string name, Rect rect)[] SpriteParts =
    {
        ("GatoCobra_Head",       new Rect(37,   567, 406, 445)),
        ("GatoCobra_Body_A",     new Rect(479,  572, 270, 411)),
        ("GatoCobra_Body_B",     new Rect(801,  572, 271, 411)),
        ("GatoCobra_Body_Glow",  new Rect(1124, 572, 273, 412)),
        ("GatoCobra_Curve_Left", new Rect(54,    90, 376, 422)),
        ("GatoCobra_Curve_Mid",  new Rect(430,   90, 288, 422)),
        ("GatoCobra_Curl",       new Rect(810,   93, 267, 360)),
        ("GatoCobra_Tail",       new Rect(1168,  58, 190, 450))
    };

    [InitializeOnLoadMethod]
    static void BuildMissingPrefabAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return;

            try
            {
                Dictionary<string, Sprite> sprites = SliceSheet();
                BuildPrefab(sprites);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("GATO_COBRA_PREFAB_AUTO_SETUP prefab=OK sprites=8");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        };
    }

    [MenuItem("Tools/Capybara vs Cats/Setup GatoCobra")]
    public static void Setup()
    {
        Dictionary<string, Sprite> sprites = SliceSheet();
        GameObject prefab = BuildPrefab(sprites);
        AddToGameplay(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("GATO_COBRA_SETUP prefab=OK sprites=8 waves=2 scene=OK");
    }

    static Dictionary<string, Sprite> SliceSheet()
    {
        TextureImporter importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException($"Spritesheet nao encontrado: {SheetPath}");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 600f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.maxTextureSize = 2048;

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        SpriteRect[] rects = SpriteParts.Select(part => new SpriteRect
        {
            name = part.name,
            rect = part.rect,
            alignment = SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f),
            spriteID = GUID.Generate()
        }).ToArray();

        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();
        AssetDatabase.ImportAsset(SheetPath, ImportAssetOptions.ForceSynchronousImport);

        Dictionary<string, Sprite> result = AssetDatabase.LoadAllAssetsAtPath(SheetPath)
            .OfType<Sprite>()
            .ToDictionary(sprite => sprite.name, sprite => sprite);
        if (result.Count != SpriteParts.Length)
            throw new InvalidOperationException($"GatoCobra: esperados {SpriteParts.Length} sprites, encontrados {result.Count}.");
        return result;
    }

    static GameObject BuildPrefab(IReadOnlyDictionary<string, Sprite> sprites)
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Enemies");

        GameObject root = new("GatoCobra");
        root.tag = "Enemy";
        SetLayerRecursively(root, 6);

        SpriteRenderer headRenderer = root.AddComponent<SpriteRenderer>();
        headRenderer.sprite = sprites["GatoCobra_Head"];
        headRenderer.sortingLayerName = "Characters";
        headRenderer.sortingOrder = 9;

        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D headCollider = root.AddComponent<CapsuleCollider2D>();
        headCollider.isTrigger = true;
        headCollider.direction = CapsuleDirection2D.Vertical;
        headCollider.size = new Vector2(0.5f, 0.58f);
        headCollider.offset = new Vector2(0f, -0.02f);

        EnemyHealth health = root.AddComponent<EnemyHealth>();
        health.health = 120;
        EnemyDeath death = root.AddComponent<EnemyDeath>();
        SerializedObject reward = new(death);
        reward.FindProperty("scoreValue").intValue = 500;
        reward.ApplyModifiedPropertiesWithoutUndo();
        GatoCobraController controller = root.AddComponent<GatoCobraController>();

        var segments = new List<Transform>();
        string[] bodyNames = { "GatoCobra_Body_A", "GatoCobra_Body_B", "GatoCobra_Body_Glow", "GatoCobra_Body_B" };
        float[] bodyY = { 0.38f, 0.66f, 0.94f, 1.22f };
        for (int i = 0; i < bodyNames.Length; i++)
        {
            Transform segment = CreateSegment(root.transform, $"Body {i + 1}", sprites[bodyNames[i]],
                new Vector3(0f, bodyY[i], 0f), 8 - i, new Vector2(0.34f, 0.48f));
            segments.Add(segment);
        }

        Transform tail = CreateSegment(root.transform, "Tail", sprites["GatoCobra_Tail"],
            new Vector3(0f, 1.58f, 0f), 3, new Vector2(0.22f, 0.56f));
        segments.Add(tail);

        GameObject firePoint = new("FirePoint");
        firePoint.transform.SetParent(root.transform, false);
        firePoint.transform.localPosition = new Vector3(0f, -0.34f, 0f);
        firePoint.layer = root.layer;

        SerializedObject settings = new(controller);
        SerializedProperty array = settings.FindProperty("bodySegments");
        array.arraySize = segments.Count;
        for (int i = 0; i < segments.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = segments[i];
        settings.FindProperty("projectilePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePath);
        settings.FindProperty("firePoint").objectReferenceValue = firePoint.transform;
        settings.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    static Transform CreateSegment(Transform parent, string name, Sprite sprite, Vector3 position,
        int sortingOrder, Vector2 colliderSize)
    {
        GameObject segment = new(name);
        segment.transform.SetParent(parent, false);
        segment.transform.localPosition = position;
        segment.layer = parent.gameObject.layer;

        SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "Characters";
        renderer.sortingOrder = sortingOrder;

        CapsuleCollider2D collider = segment.AddComponent<CapsuleCollider2D>();
        collider.isTrigger = true;
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = colliderSize;
        return segment.transform;
    }

    static void AddToGameplay(GameObject prefab)
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnemySpawner spawner = Resources.FindObjectsOfTypeAll<EnemySpawner>()
            .FirstOrDefault(candidate => candidate.gameObject.scene == scene);
        if (spawner == null) throw new InvalidOperationException("EnemySpawner nao encontrado na cena ingame.");

        List<EnemySpawner.EnemyWave> waves = spawner.waves?.ToList() ?? new List<EnemySpawner.EnemyWave>();
        waves.RemoveAll(wave => wave != null && wave.enemyPrefab == prefab);

        int bossIndex = waves.FindIndex(wave => wave != null && wave.boss);
        if (bossIndex < 0) bossIndex = waves.Count;
        waves.Insert(Mathf.Min(4, bossIndex), SnakeWave(prefab));

        bossIndex = waves.FindIndex(wave => wave != null && wave.boss);
        int beforeFinalMeteor = Mathf.Max(5, bossIndex - 1);
        waves.Insert(Mathf.Min(beforeFinalMeteor, bossIndex), SnakeWave(prefab));

        spawner.waves = waves.ToArray();
        EditorUtility.SetDirty(spawner);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static EnemySpawner.EnemyWave SnakeWave(GameObject prefab) => new()
    {
        enemyPrefab = prefab,
        enemyCount = 1,
        spawnInterval = 0f,
        boss = false
    };

    static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform) SetLayerRecursively(child.gameObject, layer);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent)) return;
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
