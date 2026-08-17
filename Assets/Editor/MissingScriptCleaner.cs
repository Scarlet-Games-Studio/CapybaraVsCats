#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptCleaner
{
    static readonly string[] PrefabFolders =
    {
        "Assets/Prefabs", "Assets/naves", "Assets/inimigos", "Assets/UI"
    };

    [InitializeOnLoadMethod]
    static void Initialize()
    {
        EditorApplication.delayCall += CleanWhenReady;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += CleanWhenReady;
        };
    }

    [MenuItem("Tools/Capybara vs Cats/Clean Missing Scripts")]
    public static void CleanWhenReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        int removed = CleanLoadedScenes();
        removed += CleanPrefabs();
        if (removed > 0)
        {
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"MISSING_SCRIPT_CLEANUP removed={removed}");
        }
    }

    static int CleanLoadedScenes()
    {
        int removed = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded || !scene.path.StartsWith("Assets/Scenes/")) continue;
            foreach (GameObject root in scene.GetRootGameObjects())
                removed += CleanHierarchy(root);
            if (removed > 0) EditorSceneManager.MarkSceneDirty(scene);
        }
        return removed;
    }

    static int CleanPrefabs()
    {
        int removed = 0;
        foreach (string path in AssetDatabase.FindAssets("t:Prefab", PrefabFolders)
                     .Select(AssetDatabase.GUIDToAssetPath))
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            int prefabRemoved = CleanHierarchy(root);
            if (prefabRemoved > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                removed += prefabRemoved;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        return removed;
    }

    static int CleanHierarchy(GameObject root)
    {
        int removed = 0;
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
            if (count == 0) continue;
            Debug.LogWarning($"Removing {count} missing script(s): {GetPath(transform)}", transform.gameObject);
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
            removed += count;
        }
        return removed;
    }

    static string GetPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
#endif
