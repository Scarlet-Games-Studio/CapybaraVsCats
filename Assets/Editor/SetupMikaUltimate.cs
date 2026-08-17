#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SetupMikaUltimate
{
    const string LaserSheet = "Assets/naves/Mika/Ultimate/Laser_mika_magicaespecialultimate-Sheet (1).png";
    const string UltimateShip = "Assets/naves/Mika/Ultimate/Laser_mika_magicaespecialultimate-Sheet sprite nave fade.png";
    const string MikaSheet = "Assets/naves/Mika/Art/Aeronave_mika.png";
    const string LaserPrefabPath = "Assets/naves/Mika/Ultimate/MikaUltimate.prefab";
    const string MikaPrefabPath = "Assets/naves/Mika/Prefabs/Mika.prefab";

    [MenuItem("Tools/Capybara vs Cats/Setup Mika Ultimate")]
    public static void Setup()
    {
        SliceLaser();
        Sprite[] laserFrames = LoadSprites(LaserSheet).Where(sprite => sprite.name.StartsWith("MikaLaser_", StringComparison.Ordinal)).ToArray();
        GameObject laserPrefab = BuildLaserPrefab(laserFrames);
        GameObject mikaPrefab = BuildMikaPrefab(laserPrefab);
        ConfigureScene(mikaPrefab);
        AssetDatabase.SaveAssets();
        Debug.Log($"MIKA_ULTIMATE_SETUP frames={laserFrames.Length} mikaPrefab=OK laserPrefab=OK scene=OK");
    }

    static void SliceLaser()
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(LaserSheet);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100f;
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var sprites = new SpriteRect[40];
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = new SpriteRect
            {
                name = $"MikaLaser_{i:00}",
                rect = new Rect(i * 672, 0, 672, 1952),
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(0.5f, 0f),
                spriteID = GUID.Generate()
            };
        }
        provider.SetSpriteRects(sprites);
        provider.Apply();
        importer.SaveAndReimport();
    }

    static Sprite[] LoadSprites(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
        .OrderBy(sprite => int.TryParse(sprite.name.Split('_').Last(), out int n) ? n : 0).ToArray();

    static GameObject BuildLaserPrefab(Sprite[] frames)
    {
        GameObject root = new("Mika Ultimate Laser");
        root.transform.localScale = new Vector3(8f, 16f, 1f);
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = frames.FirstOrDefault();
        renderer.sortingLayerName = "Characters";
        renderer.sortingOrder = 8;
        BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(5.2f, 18.5f);
        collider.offset = new Vector2(0f, 9.25f);
        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        MikaLaser mikaLaser = root.AddComponent<MikaLaser>();
        SerializedObject laserSettings = new(mikaLaser);
        laserSettings.FindProperty("impactPrefab").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/naves/Mika/Prefabs/MikaImpact.prefab");
        laserSettings.ApplyModifiedPropertiesWithoutUndo();
        SpriteSequencePlayer player = root.AddComponent<SpriteSequencePlayer>();
        SerializedObject sequence = new(player);
        SerializedProperty array = sequence.FindProperty("frames");
        array.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        sequence.FindProperty("loop").boolValue = false;
        sequence.FindProperty("slowSectionSpeed").floatValue = 0.58f;
        sequence.FindProperty("holdFrame").intValue = 35;
        sequence.FindProperty("holdDuration").floatValue = 1.5f;
        sequence.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, LaserPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject BuildMikaPrefab(GameObject laserPrefab)
    {
        GameObject hiro = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/naves/Hiro/Hiro.prefab");
        GameObject root = (GameObject)PrefabUtility.InstantiatePrefab(hiro);
        root.name = "Mika";
        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true)) UnityEngine.Object.DestroyImmediate(animator);
        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = LoadSprites(MikaSheet).FirstOrDefault();
        MikaUltimate ultimate = root.GetComponent<MikaUltimate>() ?? root.AddComponent<MikaUltimate>();
        SerializedObject so = new(ultimate);
        so.FindProperty("laserPrefab").objectReferenceValue = laserPrefab;
        so.FindProperty("ultimateShipSprite").objectReferenceValue = LoadSprites(UltimateShip).FirstOrDefault();
        so.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, MikaPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    static void ConfigureScene(GameObject mikaPrefab)
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/ingame.unity", OpenSceneMode.Single);
        GameManager manager = Resources.FindObjectsOfTypeAll<GameManager>().First(x => x.gameObject.scene == scene);
        CharacterSpawner spawner = manager.GetComponent<CharacterSpawner>() ?? manager.gameObject.AddComponent<CharacterSpawner>();
        SerializedObject spawnerSo = new(spawner);
        spawnerSo.FindProperty("mikaPrefab").objectReferenceValue = mikaPrefab;
        spawnerSo.ApplyModifiedPropertiesWithoutUndo();
        BuildUltimateButton(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void BuildUltimateButton(Scene scene)
    {
        GameObject existing = scene.GetRootGameObjects().SelectMany(All).FirstOrDefault(x => x.name == "Mika Ultimate Button");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
        Canvas canvas = Resources.FindObjectsOfTypeAll<Canvas>().First(x => x.gameObject.scene == scene);
        GameObject root = new("Mika Ultimate Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UltimateButton));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = (RectTransform)root.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-45f, 45f);
        rt.sizeDelta = new Vector2(210f, 84f);
        Image background = root.GetComponent<Image>(); background.color = new Color(0.18f, 0.03f, 0.22f, 0.95f);

        GameObject fillGo = new("Charge Fill", typeof(RectTransform), typeof(Image)); fillGo.transform.SetParent(root.transform, false);
        Image fill = fillGo.GetComponent<Image>(); fill.color = new Color(1f, 0.16f, 0.62f, 0.9f); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillAmount = 0f;
        Stretch((RectTransform)fillGo.transform);
        GameObject textGo = new("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); textGo.transform.SetParent(root.transform, false);
        TMP_Text label = textGo.GetComponent<TMP_Text>(); label.text = "0%"; label.alignment = TextAlignmentOptions.Center; label.fontSize = 25; label.fontStyle = FontStyles.Bold; label.color = Color.white;
        Stretch((RectTransform)textGo.transform);
        SerializedObject bridge = new(root.GetComponent<UltimateButton>());
        bridge.FindProperty("chargeFill").objectReferenceValue = fill;
        bridge.FindProperty("label").objectReferenceValue = label;
        bridge.ApplyModifiedPropertiesWithoutUndo();
    }

    static System.Collections.Generic.IEnumerable<GameObject> All(GameObject root)
    { yield return root; foreach (Transform child in root.transform) foreach (GameObject nested in All(child.gameObject)) yield return nested; }
    static void Stretch(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; }
}
#endif
