#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RebuildMapScene
{
    const string ScenePath = "Assets/Scenes/Map.unity";
    static readonly Color Cyan = new(.05f, .85f, 1f, 1f);
    static readonly Color Dark = new(.018f, .03f, .075f, .96f);

    [InitializeOnLoadMethod]
    static void RebuildOnceAfterCompile()
    {
        const string key = "Capybara.MapScene.v2";
        if (SessionState.GetBool(key, false)) return;
        EditorApplication.update += TryRebuild;
        void TryRebuild()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            EditorApplication.update -= TryRebuild;
            Rebuild();
            SessionState.SetBool(key, true);
        }
    }

    [MenuItem("Tools/Capybara vs Cats/Rebuild Map Scene")]
    public static void Rebuild()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);

        var camera = new GameObject("Main Camera", typeof(Camera));
        camera.tag = "MainCamera";
        camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        camera.GetComponent<Camera>().backgroundColor = new Color(.005f, .01f, .035f);

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        var canvasGo = new GameObject("Map Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f;

        Image bg = Image("Star Field", canvas.transform, Color.white); bg.sprite = Sprite("Assets/Componentes do Cenário/estrelas_assetbackgroundpng.png"); Stretch(bg.rectTransform);
        Image shade = Image("Space Shade", bg.transform, new Color(.005f,.012f,.045f,.64f)); Stretch(shade.rectTransform);
        Text("Title", shade.transform, "MAPA DA GALÁXIA", 55, new Vector2(0,455), new Vector2(800,85), Color.white);
        Text("Subtitle", shade.transform, "SELECIONE SUA PRÓXIMA MISSÃO", 21, new Vector2(0,405), new Vector2(700,42), Cyan);

        var route = Rect("Mission Route", shade.transform, Vector2.zero, new Vector2(1700,760));
        Vector2[] nodes = { new(-660,-180), new(-350,80), new(-10,-120), new(315,135), new(640,-65) };
        for (int i=0;i<nodes.Length-1;i++) DottedRoute(route, nodes[i], nodes[i+1]);

        Button earth = Planet(route, "Earth - Stage 1", "Assets/UI/Earth.png", nodes[0], 230, "TERRA", "FASE 1", false);
        Button mars = Planet(route, "Mars - Stage 2", "Assets/UI/marte.png", nodes[1], 205, "MARTE", "FASE 2", false);
        var locked = new List<Button>();
        locked.Add(Planet(route, "Unknown Planet 3", "Assets/UI/marte.png", nodes[2], 180, "PLANETA 03", "EM BREVE", true));
        locked.Add(Planet(route, "Unknown Planet 4", "Assets/UI/marte.png", nodes[3], 215, "PLANETA 04", "EM BREVE", true));
        locked.Add(Planet(route, "Jupiter", "Assets/UI/jupyter.png", nodes[4], 245, "JÚPITER", "EM BREVE", true));

        // Planetas menores preenchem o mapa e deixam clara a continuação da rota.
        locked.Add(Planet(route, "Unknown Planet 5", "Assets/UI/marte.png", new Vector2(700,265), 105, "PLANETA 05", "BLOQUEADO", true));
        locked.Add(Planet(route, "Unknown Planet 6", "Assets/UI/marte.png", new Vector2(35,285), 90, "PLANETA 06", "BLOQUEADO", true));

        Image trace = Image("Ship Trace - Current Position", route, Color.white); trace.sprite = Sprite("Assets/UI/ship_trace.png"); trace.preserveAspect = true;
        SetRect(trace.rectTransform, new Vector2(.5f,.5f), new Vector2(420,210), new Vector2(-515,-45));

        Button lobby = TextButton("Lobby Button", shade.transform, "LOBBY", new Vector2(-775,-465));
        Button exit = TextButton("Exit Button", shade.transform, "MENU", new Vector2(775,-465));
        Text("Legend", shade.transform, "PLANETAS CINZA ESTÃO BLOQUEADOS", 17, new Vector2(0,-470), new Vector2(620,42), new Color(.65f,.72f,.83f));

        var controller = new GameObject("Map Scene Controller").AddComponent<MapSceneController>();
        controller.earthButton = earth; controller.marsButton = mars; controller.lobbyButton = lobby; controller.exitButton = exit; controller.comingSoonButtons = locked.ToArray();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Map scene rebuilt: planets, routes, locked stages and ship trace added.");
    }

    static Button Planet(Transform parent, string name, string path, Vector2 pos, float size, string title, string status, bool locked)
    {
        var root = Rect(name, parent, pos, new Vector2(size+100,size+100));
        Image glow = Image("Glow", root, locked ? new Color(.16f,.18f,.23f,.38f) : new Color(0,.85f,1,.18f)); SetRect(glow.rectTransform,new Vector2(.5f,.5f),new Vector2(size+34,size+34),new Vector2(0,18));
        Image planet = Image("Planet", root, locked ? new Color(.34f,.36f,.4f,1) : Color.white); planet.sprite=Sprite(path); planet.preserveAspect=true; SetRect(planet.rectTransform,new Vector2(.5f,.5f),new Vector2(size,size),new Vector2(0,18));
        var button = planet.gameObject.AddComponent<Button>(); button.targetGraphic=planet;
        if (locked) Text("Lock", planet.transform, "LOCK", 25, Vector2.zero, new Vector2(110,55), Color.white);
        Text("Name", root, title, 22, new Vector2(0,-size*.5f-16), new Vector2(size+130,38), Color.white);
        Text("Status", root, status, 15, new Vector2(0,-size*.5f-49), new Vector2(size+130,30), locked ? new Color(.6f,.66f,.74f) : Cyan);
        return button;
    }

    static void DottedRoute(Transform parent, Vector2 from, Vector2 to)
    {
        float length=Vector2.Distance(from,to); int count=Mathf.Max(2,Mathf.FloorToInt(length/38));
        for(int i=1;i<count;i++){ Vector2 p=Vector2.Lerp(from,to,(float)i/count); Image dot=Image("Route Dot",parent,new Color(0,.78f,1,.72f)); SetRect(dot.rectTransform,new Vector2(.5f,.5f),new Vector2(18,8),p); }
    }
    static Button TextButton(string name, Transform parent, string label, Vector2 pos) { Image image=Image(name,parent,Dark); SetRect(image.rectTransform,new Vector2(.5f,.5f),new Vector2(190,62),pos); var b=image.gameObject.AddComponent<Button>(); b.targetGraphic=image; Text("Label",image.transform,label,22,Vector2.zero,new Vector2(170,45),Color.white); return b; }
    static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size) { var go=new GameObject(name,typeof(RectTransform)); var rt=(RectTransform)go.transform;rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;return rt; }
    static Image Image(string name, Transform parent, Color color) { var rt=Rect(name,parent,Vector2.zero,new Vector2(100,100));var image=rt.gameObject.AddComponent<Image>();image.color=color;return image; }
    static TMP_Text Text(string name,Transform parent,string value,float size,Vector2 pos,Vector2 dimensions,Color color){var rt=Rect(name,parent,pos,dimensions);var text=rt.gameObject.AddComponent<TextMeshProUGUI>();text.text=value;text.fontSize=size;text.fontStyle=FontStyles.Bold;text.alignment=TextAlignmentOptions.Center;text.color=color;text.enableAutoSizing=true;text.fontSizeMin=11;text.fontSizeMax=size;return text;}
    static Sprite Sprite(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
    static void SetRect(RectTransform rt,Vector2 anchor,Vector2 size,Vector2 pos){rt.anchorMin=rt.anchorMax=anchor;rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=pos;}
}
#endif
