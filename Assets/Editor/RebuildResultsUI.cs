#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RebuildResultsUI
{
    const string ScenePath = "Assets/Scenes/ingame.unity";
    static readonly Color Navy = new(0.025f, 0.055f, 0.12f, 0.98f);
    static readonly Color Cyan = new(0.05f, 0.78f, 0.92f, 1f);
    static readonly Color Gold = new(1f, 0.57f, 0.08f, 1f);

    [InitializeOnLoadMethod]
    static void RebuildOnceAfterCompile()
    {
        const string key = "Capybara.ResultsUI.v3";
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

    [MenuItem("Tools/Capybara vs Cats/Rebuild Results UI")]
    public static void Rebuild()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Canvas canvas = Resources.FindObjectsOfTypeAll<Canvas>().Where(c => c.gameObject.scene.IsValid())
            .OrderByDescending(c => c.renderMode == RenderMode.ScreenSpaceOverlay).First();

        canvas.gameObject.name = "UI Canvas";
        EnsureEventSystem();

        DeleteByName("Painel Next Stage", "Panel GameOver", "Results UI");
        DeleteLooseStars(scene);

        var resultsRoot = Rect("Results UI", canvas.transform, Vector2.zero, Vector2.zero, Vector2.one, Vector2.one);
        var stagePanel = BuildPanel(resultsRoot, "Stage Complete Panel", "FASE CONCLUÍDA", "MISSÃO COMPLETA", Gold);
        var gameOverPanel = BuildPanel(resultsRoot, "Game Over Panel", "GAME OVER", "A GALÁXIA AINDA PRECISA DE VOCÊ", Cyan);

        StageManager stage = Resources.FindObjectsOfTypeAll<StageManager>()
            .Where(s => s.gameObject.scene.IsValid()).FirstOrDefault();
        foreach (var duplicate in Resources.FindObjectsOfTypeAll<StageManager>().Where(s => s.gameObject.scene.IsValid()).ToArray())
            if (duplicate != stage) Object.DestroyImmediate(duplicate);

        ConfigureStage(stage, stagePanel);
        ConfigureGameOver(gameOverPanel);

        stagePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Results UI rebuilt and hierarchy organized successfully.");
    }

    static GameObject BuildPanel(Transform parent, string name, string title, string subtitle, Color accent)
    {
        var overlay = ImageRect(name, parent, new Color(0.005f, 0.012f, 0.035f, 0.86f));
        Stretch(overlay.rectTransform);

        var card = ImageRect("Card", overlay.transform, Color.white);
        card.sprite = LoadSprite("Assets/UI/panel next game 1.png");
        card.preserveAspect = true;
        SetRect(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(650, 900), Vector2.zero);

        var accentBar = ImageRect("Accent", card.transform, accent);
        SetRect(accentBar.rectTransform, new Vector2(0.5f, 1), new Vector2(620, 8), new Vector2(0, -28));

        Text("Title", card.transform, title, 58, FontStyles.Bold, Color.white, new Vector2(0, 340), new Vector2(570, 80));
        Text("Subtitle", card.transform, subtitle, 21, FontStyles.Bold, accent, new Vector2(0, 285), new Vector2(570, 45));
        return overlay.gameObject;
    }

    static void ConfigureStage(StageManager stage, GameObject panel)
    {
        if (stage == null) throw new System.InvalidOperationException("Configured StageManager not found.");
        Transform card = panel.transform.Find("Card");
        var planet = ImageRect("Marte", card, Color.white); planet.sprite = LoadSprite("Assets/UI/marte.png"); planet.preserveAspect = true;
        SetRect(planet.rectTransform, new Vector2(.5f,.5f), new Vector2(150,150), new Vector2(0,215));
        var score = Text("Score", card, "SCORE  0", 32, FontStyles.Bold, Color.white, new Vector2(0, 125), new Vector2(560, 55));

        var meter = Rect("Stars Meter", card, new Vector2(0, 45), new Vector2(460, 150));
        var ghost = ImageRect("Stars Background", meter, new Color(0.28f, 0.31f, 0.38f, 0.42f));
        var fill = ImageRect("Stars Fill", meter, Color.white);
        Sprite stars = LoadSprite("Assets/UI/estrela.png");
        foreach (var image in new[] { ghost, fill }) { image.sprite = stars; image.preserveAspect = true; Stretch(image.rectTransform); }
        fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 0;
        var value = Text("Stars Value", card, "0 / 3 ESTRELAS", 19, FontStyles.Bold, new Color(0.82f, 0.87f, 0.94f), new Vector2(0, -35), new Vector2(500, 36));

        Button next = SpriteButton("Next Stage Button", card, "Assets/UI/Nexstagebutton.jpeg", new Vector2(0, -125));
        Button map = SpriteButton("Map Button", card, "Assets/UI/MapButton.jpeg", new Vector2(0, -215));
        Button lobby = SpriteButton("Lobby Button", card, "Assets/UI/LobbyButton.jpeg", new Vector2(0, -305));
        Button exit = Button("Exit Button", card, "SAIR", new Color(0.08f, 0.12f, 0.22f, .98f), new Vector2(0, -390), new Vector2(470, 58));

        stage.stageCompleteUI = panel;
        stage.scoreViewText = score;
        stage.stars = meter.gameObject;
        stage.starFill = fill;
        stage.starsValueText = value;
        stage.nextButton = next;
        stage.mapButton = map;
        stage.lobbyButton = lobby;
        stage.exitButton = exit;
        stage.comingSoonSceneName = "ComingSoon";
        stage.mapSceneName = "Map";
        stage.lobbySceneName = "Lobby";
        stage.mainMenuSceneName = "MainMenu";
    }

    static void ConfigureGameOver(GameObject panel)
    {
        Transform card = panel.transform.Find("Card");
        var score = Text("Final Score", card, "SCORE  0", 42, FontStyles.Bold, Color.white, new Vector2(0, 85), new Vector2(620, 70));
        Text("Hint", card, "Tente novamente e supere sua melhor pontuação.", 23, FontStyles.Normal, new Color(0.75f, 0.81f, 0.9f), new Vector2(0, 5), new Vector2(620, 70));
        Button restart = Button("Restart Button", card, "TENTAR NOVAMENTE", Cyan, new Vector2(0, -135), new Vector2(500, 84));
        Button exit = Button("Exit Button", card, "VOLTAR AO MENU", new Color(0.12f, 0.18f, 0.29f, 1), new Vector2(0, -235), new Vector2(500, 72));

        GameOverScreen screen = Resources.FindObjectsOfTypeAll<GameOverScreen>().FirstOrDefault(s => s.gameObject.scene.IsValid());
        if (screen == null) screen = panel.AddComponent<GameOverScreen>();
        screen.gameOverUI = panel;
        screen.exitButton = exit;
        screen.restartButton = restart;
        screen.finalScoreText = score;
    }

    static void DeleteLooseStars(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Estrelas" || root.name.StartsWith("estrelas (")) Object.DestroyImmediate(root);
    }

    static void DeleteByName(params string[] names)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>().Where(g => names.Contains(g.name) && g.scene.IsValid()).ToArray())
            Object.DestroyImmediate(go);
    }

    static void EnsureEventSystem()
    {
        if (Resources.FindObjectsOfTypeAll<EventSystem>().Any(e => e.gameObject.scene.IsValid())) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        go.transform.SetAsFirstSibling();
    }

    static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size) => Rect(name, parent, pos, new Vector2(.5f,.5f), new Vector2(.5f,.5f), size);
    static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform)); var rt = (RectTransform)go.transform; rt.SetParent(parent, false);
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.anchoredPosition = pos; rt.sizeDelta = size; return rt;
    }
    static Image ImageRect(string name, Transform parent, Color color)
    { var rt = Rect(name, parent, Vector2.zero, new Vector2(100,100)); var image = rt.gameObject.AddComponent<Image>(); image.color = color; return image; }
    static TMP_Text Text(string name, Transform parent, string value, float size, FontStyles style, Color color, Vector2 pos, Vector2 dimensions)
    { var rt=Rect(name,parent,pos,dimensions); var t=rt.gameObject.AddComponent<TextMeshProUGUI>(); t.text=value;t.fontSize=size;t.fontStyle=style;t.color=color;t.alignment=TextAlignmentOptions.Center;t.enableAutoSizing=true;t.fontSizeMin=14;t.fontSizeMax=size;return t; }
    static Button Button(string name, Transform parent, string label, Color color, Vector2 pos, Vector2 size)
    { var image=ImageRect(name,parent,color);SetRect(image.rectTransform,new Vector2(.5f,.5f),size,pos);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;Text("Label",image.transform,label,25,FontStyles.Bold,Color.white,Vector2.zero,size-new Vector2(30,20));return b; }
    static Button SpriteButton(string name, Transform parent, string path, Vector2 pos)
    { var image=ImageRect(name,parent,Color.white);image.sprite=LoadSprite(path);image.preserveAspect=true;SetRect(image.rectTransform,new Vector2(.5f,.5f),new Vector2(500,78),pos);var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;return b; }
    static Sprite LoadSprite(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    static void Stretch(RectTransform rt) { rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero; }
    static void SetRect(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 pos) { rt.anchorMin=anchor;rt.anchorMax=anchor;rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=pos; }
}
#endif
