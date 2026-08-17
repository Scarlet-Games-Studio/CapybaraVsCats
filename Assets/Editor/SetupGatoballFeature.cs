#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupGatoballFeature
{
    const string GatoballPath = "Assets/inimigos/Minion-kamikaze-Gatoball.prefab";
    const string DropPath = "Assets/Prefabs/ShieldPowerupDROP.prefab";
    const string ScenePath = "Assets/Scenes/ingame.unity";

    [MenuItem("Tools/Capybara vs Cats/Setup Gatoball and Shields")]
    public static void Setup()
    {
        GameObject hiro = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/naves/power ups/ShieldPowerup/ShieldHiro/HiroShield.prefab");
        GameObject mika = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/naves/power ups/ShieldPowerup/ShieldRosamika/ShieldPowerUp.prefab");
        GameObject edge = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/naves/power ups/ShieldPowerup/ShieldEdge/EdgePOWER.prefab");
        GameObject drop = BuildDrop(hiro, mika, edge);
        ConfigureGatoball(drop);
        ConfigurePlayerProjectile();
        AddWave();
        AssetDatabase.SaveAssets();
        Debug.Log("Gatoball kamikaze, shield drop and gameplay wave configured successfully.");
    }

    static GameObject BuildDrop(GameObject hiro, GameObject mika, GameObject edge)
    {
        GameObject root = new("Shield Powerup DROP");
        root.layer = LayerMask.NameToLayer("Default");
        var renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/naves/power ups/ShieldPowerup/ShieldHiro/hiroescudo01.png").OfType<Sprite>().FirstOrDefault();
        renderer.sortingLayerName = "Characters";
        renderer.sortingOrder = 5;
        renderer.color = new Color(0.25f, 0.95f, 1f, 1f);
        root.transform.localScale = Vector3.one * 0.35f;
        var collider = root.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = 1.2f;
        var body = root.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0;
        var pickup = root.AddComponent<ShieldPickup>();
        SerializedObject so = new(pickup);
        so.FindProperty("hiroShield").objectReferenceValue = hiro;
        so.FindProperty("mikaShield").objectReferenceValue = mika;
        so.FindProperty("edgeShield").objectReferenceValue = edge;
        so.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, DropPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void ConfigureGatoball(GameObject drop)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GatoballPath);
        foreach (var oldAi in root.GetComponents<EnemyAI>()) Object.DestroyImmediate(oldAi);
        if (root.GetComponent<GatoballKamikaze>() == null) root.AddComponent<GatoballKamikaze>();
        GatoballDrop dropper = root.GetComponent<GatoballDrop>();
        if (dropper == null) dropper = root.AddComponent<GatoballDrop>();
        SerializedObject so = new(dropper);
        so.FindProperty("shieldDropPrefab").objectReferenceValue = drop;
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, GatoballPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void ConfigurePlayerProjectile()
    {
        const string path = "Assets/Prefabs/PlayerProjectile.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        Collider2D collider = root.GetComponent<Collider2D>();
        if (collider != null) collider.isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void AddWave()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EnemySpawner spawner = Resources.FindObjectsOfTypeAll<EnemySpawner>().First(s => s.gameObject.scene == scene);
        GameObject gatoball = AssetDatabase.LoadAssetAtPath<GameObject>(GatoballPath);
        if (!spawner.waves.Any(w => w.enemyPrefab == gatoball))
        {
            var list = spawner.waves.ToList();
            int bossIndex = list.FindIndex(w => w.boss);
            var wave = new EnemySpawner.EnemyWave { enemyPrefab = gatoball, enemyCount = 4, spawnInterval = 1.1f, boss = false };
            list.Insert(bossIndex >= 0 ? bossIndex : list.Count, wave);
            spawner.waves = list.ToArray();
            EditorUtility.SetDirty(spawner);
        }
        EditorSceneManager.SaveScene(scene);
    }
}
#endif
