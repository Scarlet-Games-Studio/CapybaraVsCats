#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UpgradeMainMenu
{
    const string ScenePath="Assets/Scenes/MainMenu.unity";

    [MenuItem("Tools/Capybara vs Cats/Upgrade Main Menu Mobile")]
    public static void Upgrade()
    {
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        Canvas canvas=Resources.FindObjectsOfTypeAll<Canvas>().First(c=>c.gameObject.scene==scene);
        CanvasScaler scaler=canvas.GetComponent<CanvasScaler>()??canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;

        MainMenuController controller=Resources.FindObjectsOfTypeAll<MainMenuController>().FirstOrDefault(x=>x.gameObject.scene==scene);
        if(controller==null)controller=new GameObject("Main Menu Controller").AddComponent<MainMenuController>();
        if(string.IsNullOrWhiteSpace(controller.inGameSceneName)||controller.inGameSceneName=="InGame")controller.inGameSceneName="CharacterSelectV2";

        Button[] buttons=Resources.FindObjectsOfTypeAll<Button>().Where(b=>b.gameObject.scene==scene).OrderBy(b=>b.transform.GetSiblingIndex()).ToArray();
        for(int i=0;i<buttons.Length;i++)
        {
            MenuButtonMotion motion=buttons[i].GetComponent<MenuButtonMotion>()??buttons[i].gameObject.AddComponent<MenuButtonMotion>();
            motion.Configure(.08f*i,buttons[i]==controller.startButton);
            Navigation navigation=buttons[i].navigation;navigation.mode=Navigation.Mode.Automatic;buttons[i].navigation=navigation;
        }

        GameObject logo=Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g=>g.scene==scene&&(g.name=="Logotipo"||g.name.Contains("Logo")));
        if(logo!=null&&logo.GetComponent<MenuAmbientMotion>()==null)logo.AddComponent<MenuAmbientMotion>();
        EditorUtility.SetDirty(controller);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("Main Menu mobile polish applied without changing its buttons.");
    }

}
#endif
