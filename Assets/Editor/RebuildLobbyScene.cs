#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RebuildLobbyScene
{
    const string ScenePath = "Assets/Scenes/Lobby.unity";
    static readonly Color Navy = new(.018f,.035f,.09f,.96f);
    static readonly Color Cyan = new(.04f,.85f,1f,1f);

    [MenuItem("Tools/Capybara vs Cats/Rebuild Lobby Scene")]
    public static void Rebuild()
    {
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        foreach(GameObject root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);
        var camera=new GameObject("Main Camera",typeof(Camera)); camera.tag="MainCamera";camera.GetComponent<Camera>().backgroundColor=new Color(.005f,.01f,.03f);
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        var canvasGo=new GameObject("Lobby Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        Image bg=Image("Background",canvas.transform,Color.white);bg.sprite=Sprite("Assets/Componentes do Cenário/ceu_4.png");bg.type=UnityEngine.UI.Image.Type.Simple;bg.preserveAspect=false;Stretch(bg.rectTransform);
        Image stars=Image("Stars",bg.transform,new Color(1,1,1,.72f));stars.sprite=Sprite("Assets/Componentes do Cenário/estrelas.png");Stretch(stars.rectTransform);
        Image shade=Image("Readability Shade",bg.transform,new Color(.005f,.012f,.04f,.34f));Stretch(shade.rectTransform);
        Image logo=Image("Capybara vs Cats Logo",shade.transform,Color.white);logo.sprite=Sprite("Assets/UI/15 Sem Título_20241107162905.png");logo.preserveAspect=true;SetRect(logo.rectTransform,new Vector2(.5f,.5f),new Vector2(270,150),new Vector2(760,430));

        var profile=Rect("Most Used Character",shade.transform,new Vector2(-575,-20),new Vector2(780,980));
        Image splash=Image("Character Splash",profile,Color.white);splash.preserveAspect=true;SetRect(splash.rectTransform,new Vector2(.5f,.5f),new Vector2(820,900),new Vector2(-20,-20));
        Image profileBadge=Image("Profile Badge",profile,new Color(.015f,.06f,.13f,.94f));SetRect(profileBadge.rectTransform,new Vector2(.5f,.5f),new Vector2(520,122),new Vector2(5,-390));
        Text("Header",profileBadge.transform,"PERSONAGEM MAIS USADO",17,new Vector2(0,35),new Vector2(470,30),Cyan);
        TMP_Text charName=Text("Character Name",profileBadge.transform,"HIRO",29,new Vector2(0,2),new Vector2(470,42),Color.white);
        TMP_Text best=Text("Best Score",profileBadge.transform,"RECORDE  0",18,new Vector2(0,-35),new Vector2(470,30),new Color(1,.67f,.18f));

        Image ranking=Image("Ranking Panel",shade.transform,Color.white);ranking.sprite=Sprite("Assets/UI/GameOverPainel.png");ranking.preserveAspect=false;SetRect(ranking.rectTransform,new Vector2(.5f,.5f),new Vector2(1030,830),new Vector2(390,-25));
        Image rankHeader=Image("Ranking Art Header",ranking.transform,Color.white);rankHeader.sprite=Sprite("Assets/UI/Ranking.jpeg");rankHeader.preserveAspect=false;SetRect(rankHeader.rectTransform,new Vector2(.5f,.5f),new Vector2(650,112),new Vector2(0,330));
        TMP_Text rankTitle=Text("Ranking Title",ranking.transform,"RANKING LOCAL",27,new Vector2(0,258),new Vector2(760,45),Color.white);
        TMP_Text status=Text("Ranking Status",ranking.transform,"RECORDES SALVOS NESTE DISPOSITIVO",14,new Vector2(0,220),new Vector2(760,28),new Color(.55f,.9f,1));
        Button world=Tab("World Tab",ranking.transform,"MUNDIAL",new Vector2(-250,177));Button regional=Tab("Regional Tab",ranking.transform,"REGIONAL",new Vector2(0,177));Button local=Tab("Local Tab",ranking.transform,"LOCAL",new Vector2(250,177));
        TMP_Text[] rows=new TMP_Text[10];for(int i=0;i<rows.Length;i++){Image row=Image($"Rank {i+1:00}",ranking.transform,i%2==0?new Color(.015f,.12f,.22f,.82f):new Color(.01f,.075f,.16f,.76f));SetRect(row.rectTransform,new Vector2(.5f,.5f),new Vector2(760,42),new Vector2(0,112-i*44));rows[i]=Text("Value",row.transform,$"{i+1:00}  ---          0",18,Vector2.zero,new Vector2(700,34),Color.white);}
        Button map=SpriteButton("Map Button",shade.transform,"Assets/UI/MapButton.jpeg",new Vector2(615,-465),new Vector2(300,70));Button exit=SpriteButton("Exit Button",shade.transform,"Assets/UI/13 Sem Título_20241201181453.png",new Vector2(300,-465),new Vector2(300,70));

        var manager=new GameObject("Lobby Manager").AddComponent<LobbyManager>();manager.characterSplash=splash;manager.characterNameText=charName;manager.bestScoreText=best;manager.hiroSplash=Sprite("Assets/UI/1000059229.jpg");manager.mikaSplash=Sprite("Assets/UI/grtSplashArt.png");manager.edgeSplash=Sprite("Assets/naves/Jack/JackSplashArt.png");manager.rankingTitleText=rankTitle;manager.rankingStatusText=status;manager.rankingRows=rows;manager.worldButton=world;manager.regionalButton=regional;manager.localButton=local;manager.mapButton=map;manager.exitButton=exit;
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Debug.Log("Lobby rebuilt with character profile and Local/Regional/World rankings.");
    }

    [InitializeOnLoadMethod] static void Auto(){const string key="Capybara.Lobby.v3";if(SessionState.GetBool(key,false))return;EditorApplication.update+=Try;void Try(){if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;EditorApplication.update-=Try;Rebuild();SessionState.SetBool(key,true);}}
    static Button Tab(string n,Transform p,string label,Vector2 pos)=>Button(n,p,label,pos,new Vector2(220,54),new Color(.035f,.18f,.25f,1));
    static Button SpriteButton(string n,Transform p,string path,Vector2 pos,Vector2 size){Image image=Image(n,p,Color.white);image.sprite=Sprite(path);image.preserveAspect=false;SetRect(image.rectTransform,new Vector2(.5f,.5f),size,pos);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;return b;}
    static Button Button(string n,Transform p,string label,Vector2 pos,Vector2 size,Color color){Image image=Image(n,p,color);SetRect(image.rectTransform,new Vector2(.5f,.5f),size,pos);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;Text("Label",image.transform,label,19,Vector2.zero,size-new Vector2(20,12),Color.white);return b;}
    static RectTransform Rect(string n,Transform p,Vector2 pos,Vector2 size){var go=new GameObject(n,typeof(RectTransform));var rt=(RectTransform)go.transform;rt.SetParent(p,false);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;return rt;}
    static Image Image(string n,Transform p,Color c){var rt=Rect(n,p,Vector2.zero,new Vector2(100,100));var image=rt.gameObject.AddComponent<Image>();image.color=c;return image;}
    static TMP_Text Text(string n,Transform p,string value,float size,Vector2 pos,Vector2 dimensions,Color color){var rt=Rect(n,p,pos,dimensions);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.text=value;text.fontSize=size;text.fontStyle=FontStyles.Bold;text.alignment=TextAlignmentOptions.Center;text.color=color;text.enableAutoSizing=true;text.fontSizeMin=11;text.fontSizeMax=size;return text;}
    static Sprite Sprite(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
    static void SetRect(RectTransform rt,Vector2 anchor,Vector2 size,Vector2 pos){rt.anchorMin=rt.anchorMax=anchor;rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=pos;}
}
#endif
