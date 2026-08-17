#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CharacterSelectDiagnostics
{
    const string ScenePath = "Assets/Scenes/CharacterSelectV2.unity";

    [MenuItem("Tools/Capybara vs Cats/Validate Character Select")]
    public static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject[] objects = scene.GetRootGameObjects().SelectMany(GetHierarchy).ToArray();
        var missing = objects.Where(go => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0).ToArray();

        UI_Spin controller = Resources.FindObjectsOfTypeAll<UI_Spin>()
            .FirstOrDefault(x => x.gameObject.scene == scene && x.gameObject.name == "Background2");
        if (controller == null)
            controller = Resources.FindObjectsOfTypeAll<UI_Spin>().FirstOrDefault(x => x.gameObject.scene == scene);

        int brokenEvents = 0;
        foreach (Button button in objects.SelectMany(go => go.GetComponents<Button>()))
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                Object target = button.onClick.GetPersistentTarget(i);
                string method = button.onClick.GetPersistentMethodName(i);
                if (target == null || string.IsNullOrEmpty(method) || target.GetType().GetMethod(method) == null)
                {
                    brokenEvents++;
                    Debug.LogError($"Broken Character Select event: {button.name} -> {method}", button);
                }
            }
        }

        foreach (GameObject go in missing)
        {
            if (go.name == "BackgroundMusic")
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                EditorUtility.SetDirty(go);
                Debug.Log("Removed obsolete FMOD StudioEventEmitter from CharacterSelectV2/BackgroundMusic.", go);
            }
            else
            {
                Debug.LogError($"Missing script on Character Select object: {GetPath(go.transform)}", go);
            }
        }

        if (missing.Length > 0) EditorSceneManager.SaveScene(scene);
        int remainingMissing = objects.Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount);

        Debug.Log($"CHARACTER_SELECT_VALIDATION controller={(controller != null ? "OK" : "MISSING")} missingScripts={remainingMissing} brokenButtonEvents={brokenEvents}");
    }

    static System.Collections.Generic.IEnumerable<GameObject> GetHierarchy(GameObject root)
    {
        yield return root;
        foreach (Transform child in root.transform)
            foreach (GameObject nested in GetHierarchy(child.gameObject)) yield return nested;
    }

    static string GetPath(Transform transform) => transform.parent == null ? transform.name : GetPath(transform.parent) + "/" + transform.name;
}
#endif
