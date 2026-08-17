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

        foreach(GameObject old in Resources.FindObjectsOfTypeAll<GameObject>().Where(g=>g.scene==scene&&g.name=="Returning Player Shortcuts").ToArray())Object.DestroyImmediate(old);
        RectTransform shortcuts=Rect("Returning Player Shortcuts",canvas.transform,new Vector2(0,-405),new Vector2(760,150));
        Button lobby=SpriteButton("Lobby Button",shortcuts,"Assets/UI/LobbyButton.jpeg",new Vector2(-195,0),new Vector2(350,88));
        Button map=SpriteButton("Map Button",shortcuts,"Assets/UI/MapButton.jpeg",new Vector2(195,0),new Vector2(350,88));

        MainMenuController controller=Resources.FindObjectsOfTypeAll<MainMenuController>().FirstOrDefault(x=>x.gameObject.scene==scene);
        if(controller==null)controller=new GameObject("Main Menu Controller").AddComponent<MainMenuController>();
        controller.returningPlayerButtons=shortcuts.gameObject;controller.lobbyButton=lobby;controller.mapButton=map;
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
        shortcuts.gameObject.SetActive(false);
        EditorUtility.SetDirty(controller);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Debug.Log("Main Menu upgraded for mobile with returning-player shortcuts and DOTween motion.");
    }

    [InitializeOnLoadMethod]static void Auto(){const string key="Capybara.MainMenu.Mobile.v1";if(SessionState.GetBool(key,false))return;EditorApplication.update+=Try;void Try(){if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;EditorApplication.update-=Try;Upgrade();SessionState.SetBool(key,true);}}
    static RectTransform Rect(string n,Transform p,Vector2 pos,Vector2 size){var go=new GameObject(n,typeof(RectTransform));var rt=(RectTransform)go.transform;rt.SetParent(p,false);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;return rt;}
    static Button SpriteButton(string n,Transform p,string path,Vector2 pos,Vector2 size){var rt=Rect(n,p,pos,size);var image=rt.gameObject.AddComponent<Image>();image.sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();image.preserveAspect=true;var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=image;return b;}
}
#endif
