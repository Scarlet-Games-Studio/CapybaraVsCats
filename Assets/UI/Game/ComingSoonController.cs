using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ComingSoonController : MonoBehaviour
{
    [SerializeField, Min(1f)] float returnDelay = 5f;
    [SerializeField] string mainMenuSceneName = "MainMenu";

    void Awake() => BuildScreen();
    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(returnDelay);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void BuildScreen()
    {
        var canvasGo = new GameObject("Coming Soon Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
        Image bg = Image("Space Background", canvas.transform, new Color(0.015f, 0.025f, 0.09f, 1f)); Stretch(bg.rectTransform);
        Text("Title", bg.transform, "EM BREVE", 104, new Vector2(0, 55), Color.white);
        Text("Subtitle", bg.transform, "NOVAS MISSÕES ESTÃO A CAMINHO", 31, new Vector2(0, -65), new Color(.18f, .9f, 1f));
        Text("Return", bg.transform, "Voltando ao menu principal...", 21, new Vector2(0, -145), new Color(.7f, .76f, .9f));
    }

    static Image Image(string name, Transform parent, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var image = go.GetComponent<Image>(); image.color = color; return image; }
    static void Text(string name, Transform parent, string value, float size, Vector2 pos, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f); rect.sizeDelta = new Vector2(1500,140); rect.anchoredPosition = pos; var text = go.GetComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.fontStyle = FontStyles.Bold; text.alignment = TextAlignmentOptions.Center; text.color = color; }
    static void Stretch(RectTransform rect) { rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=rect.offsetMax=Vector2.zero; }
}
