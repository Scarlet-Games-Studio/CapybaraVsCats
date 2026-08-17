#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class SetupMikaWeapon
{
    const string ProjectileSheet = "Assets/naves/Mika/Art/Projetil_mikamagica-Sheet.png";
    const string ImpactSheet = "Assets/naves/Mika/Art/impact_mika_magica-Sheet.png";
    const string ProjectilePrefabPath = "Assets/naves/Mika/Prefabs/MikaProjectile.prefab";
    const string ImpactPrefabPath = "Assets/naves/Mika/Prefabs/MikaImpact.prefab";
    const string ProjectileClipPath = "Assets/naves/Mika/Animations/MikaProjectile.anim";
    const string ImpactClipPath = "Assets/naves/Mika/Animations/MikaImpact.anim";
    const string ProjectileControllerPath = "Assets/naves/Mika/Animations/MikaProjectile.controller";
    const string ImpactControllerPath = "Assets/naves/Mika/Animations/MikaImpact.controller";
    const string MikaPrefabPath = "Assets/naves/Mika/Prefabs/Mika.prefab";

    [InitializeOnLoadMethod]
    static void SetupWhenScriptsReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath) == null)
                Setup();
        };
    }

    [MenuItem("Tools/Capybara vs Cats/Setup Mika Weapon")]
    public static void Setup()
    {
        Sprite[] projectileFrames = SliceSheet(ProjectileSheet, "MikaProjectile", 4);
        Sprite[] impactFrames = SliceSheet(ImpactSheet, "MikaImpact", 4);

        GameObject impact = BuildImpact(impactFrames);
        GameObject projectile = BuildProjectile(projectileFrames, impact);
        LinkMikaPrefab(projectile);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"MIKA_WEAPON_SETUP projectile={projectile.name} impact={impact.name} frames=4+4");
    }

    static Sprite[] SliceSheet(string path, string prefix, int frameCount)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        float width = texture.width / (float)frameCount;
        SpriteRect[] rects = Enumerable.Range(0, frameCount).Select(i => new SpriteRect
        {
            name = $"{prefix}_{i:00}",
            rect = new Rect(i * width, 0f, width, texture.height),
            alignment = SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f),
            spriteID = GUID.Generate()
        }).ToArray();

        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .Where(s => s.name.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(s => s.name).ToArray();
        if (frames.Length != frameCount)
            throw new InvalidOperationException($"{path}: esperados {frameCount} frames, encontrados {frames.Length}.");
        return frames;
    }

    static AnimationClip BuildClip(string path, Sprite[] frames, bool loop)
    {
        AnimationClip old = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (old != null) AssetDatabase.DeleteAsset(path);

        var clip = new AnimationClip { frameRate = 12f, name = System.IO.Path.GetFileNameWithoutExtension(path) };
        var binding = new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" };
        var keys = frames.Select((sprite, i) => new ObjectReferenceKeyframe
        {
            time = i / clip.frameRate,
            value = sprite
        }).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { loopTime = loop });
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static RuntimeAnimatorController BuildController(string path, AnimationClip clip)
    {
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path) != null)
            AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.layers[0].stateMachine.AddState(clip.name).motion = clip;
        return controller;
    }

    static GameObject BuildImpact(Sprite[] frames)
    {
        AnimationClip clip = BuildClip(ImpactClipPath, frames, false);
        RuntimeAnimatorController controller = BuildController(ImpactControllerPath, clip);
        var root = new GameObject("MikaImpact", typeof(SpriteRenderer), typeof(Animator));
        root.transform.localScale = Vector3.one * 0.35f;
        root.GetComponent<SpriteRenderer>().sprite = frames[0];
        root.GetComponent<SpriteRenderer>().sortingOrder = 10;
        root.GetComponent<Animator>().runtimeAnimatorController = controller;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ImpactPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    static GameObject BuildProjectile(Sprite[] frames, GameObject impact)
    {
        AnimationClip clip = BuildClip(ProjectileClipPath, frames, true);
        RuntimeAnimatorController controller = BuildController(ProjectileControllerPath, clip);
        var root = new GameObject("MikaProjectile", typeof(SpriteRenderer), typeof(Projectile), typeof(Animator), typeof(BoxCollider2D));
        root.tag = "PlayerProjectile";
        root.transform.localScale = Vector3.one * 0.2f;
        root.GetComponent<SpriteRenderer>().sprite = frames[0];
        root.GetComponent<SpriteRenderer>().sortingOrder = 9;
        root.GetComponent<Animator>().runtimeAnimatorController = controller;
        root.GetComponent<BoxCollider2D>().isTrigger = true;
        root.GetComponent<BoxCollider2D>().size = new Vector2(0.7f, 2.1f);
        root.GetComponent<Projectile>().explosionPrefab = impact;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    static void LinkMikaPrefab(GameObject projectile)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(MikaPrefabPath);
        PlayerController controller = root.GetComponent<PlayerController>();
        if (controller == null)
            throw new InvalidOperationException("Mika prefab não possui PlayerController.");
        controller.projectilePrefab = projectile;
        EditorUtility.SetDirty(controller);
        PrefabUtility.SaveAsPrefabAsset(root, MikaPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
#endif
