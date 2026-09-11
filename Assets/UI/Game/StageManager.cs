using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [Header("Stage Complete UI")]
    public GameObject stageCompleteUI;
    public TMP_Text scoreViewText;
    public GameObject stars;
    public Image starFill;
    public TMP_Text starsValueText;
    public Button nextButton;
    public Button mapButton;
    public Button lobbyButton;
    public Button exitButton;

    [Header("Cenas")]
    [Tooltip("Cena seguinte explícita. Se ficar vazia, ingame avança para stage2 e as demais para ComingSoon.")]
    public string nextSceneName;
    public string comingSoonSceneName = "ComingSoon";
    public string mapSceneName = "Map";
    public string lobbySceneName = "Lobby";
    public string mainMenuSceneName = "MainMenu";

    [Header("Pontuação")]
    [Min(1)] public int maxScore = 10000;

    bool completed;
    bool transitioning;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (stageCompleteUI != null) stageCompleteUI.SetActive(false);
        ConfigureButton(nextButton, GoToNextStage);
        ConfigureButton(mapButton, () => LoadScene(mapSceneName));
        ConfigureButton(lobbyButton, () => LoadScene(lobbySceneName));
        ConfigureButton(exitButton, () => LoadScene(mainMenuSceneName));
    }

    void OnDestroy()
    {
        if (nextButton != null) nextButton.onClick.RemoveListener(GoToNextStage);
    }

    static void ConfigureButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    public void OnStageComplete()
    {
        if (completed) return;
        completed = true;

        ProgressManager.SaveProgress();
        ProgressManager.SaveStageScore(ScoreManager.score);

        if (stageCompleteUI == null)
        {
            Debug.LogWarning("A UI de conclusão da fase não está configurada.", this);
            Time.timeScale = 0f;
            StartCoroutine(AutoAdvanceWithoutPanel());
            return;
        }

        stageCompleteUI.SetActive(true);
        stageCompleteUI.transform.SetAsLastSibling();
        if (scoreViewText != null) scoreViewText.text = $"SCORE  {ScoreManager.score:N0}";
        UpdateStars(ScoreManager.score, maxScore);
        SetNavigationInteractable(true);
        Time.timeScale = 0f;
    }

    IEnumerator AutoAdvanceWithoutPanel()
    {
        yield return new WaitForSecondsRealtime(1.25f);
        if (!transitioning) GoToNextStage();
    }

    public void EnableNextButton()
    {
        if (nextButton != null) nextButton.interactable = true;
    }

    public void GoToNextStage()
    {
        if (transitioning) return;
        transitioning = true;
        SetNavigationInteractable(false);
        ProgressManager.SaveProgress();
        ProgressManager.SaveStageScore(ScoreManager.score);

        string destination = ResolveNextScene(SceneManager.GetActiveScene().name);
        RewardedAdBridge.Show(this, () => LoadScene(destination));
    }

    string ResolveNextScene(string currentScene)
    {
        if (!string.IsNullOrWhiteSpace(nextSceneName) &&
            !string.Equals(nextSceneName, currentScene, StringComparison.OrdinalIgnoreCase) &&
            Application.CanStreamedLevelBeLoaded(nextSceneName))
            return nextSceneName;

        if (string.Equals(currentScene, "ingame", StringComparison.OrdinalIgnoreCase) &&
            Application.CanStreamedLevelBeLoaded("stage2"))
            return "stage2";

        return comingSoonSceneName;
    }

    void SetNavigationInteractable(bool value)
    {
        if (nextButton != null) nextButton.interactable = value;
        if (mapButton != null) mapButton.interactable = value;
        if (lobbyButton != null) lobbyButton.interactable = value;
        if (exitButton != null) exitButton.interactable = value;
    }

    void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"A cena '{sceneName}' não está habilitada no Build Settings.");
            transitioning = false;
            SetNavigationInteractable(true);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    void UpdateStars(int score, int possibleScore)
    {
        float ratio = possibleScore > 0 ? Mathf.Clamp01((float)score / possibleScore) : 0f;
        float earnedStars = Mathf.Round(ratio * 6f) * 0.5f;
        if (score > 0) earnedStars = Mathf.Max(0.5f, earnedStars);
        if (starFill != null) starFill.fillAmount = earnedStars / 3f;
        if (starsValueText != null) starsValueText.text = $"{earnedStars:0.#} / 3 ESTRELAS";
    }
}

public static class RewardedAdBridge
{
    public static void Show(MonoBehaviour runner, Action onFinished)
    {
        if (runner == null) { onFinished?.Invoke(); return; }
        runner.StartCoroutine(ShowFallbackAd(onFinished));
    }

    static IEnumerator ShowFallbackAd(Action onFinished)
    {
        var canvasObject = new GameObject("Ad Interstitial", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = short.MaxValue;
        var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
        var background = CreateImage("Background", canvas.transform, new Color(0.015f, 0.025f, 0.07f, 0.98f)); Stretch(background.rectTransform);
        CreateText("Label", background.transform, "ANÚNCIO", 54, new Vector2(0, 45), Color.white);
        CreateText("Info", background.transform, "Próxima missão sendo preparada...", 25, new Vector2(0, -35), new Color(0.35f, 0.9f, 1f));
        yield return new WaitForSecondsRealtime(2.5f);
        if (canvasObject != null) UnityEngine.Object.Destroy(canvasObject);
        onFinished?.Invoke();
    }

    static Image CreateImage(string name, Transform parent, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false); var image = go.GetComponent<Image>(); image.color = color; return image; }
    static void CreateText(string name, Transform parent, string value, float size, Vector2 position, Color color) { var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(800, 90); rect.anchoredPosition = position; var text = go.GetComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.fontStyle = FontStyles.Bold; text.alignment = TextAlignmentOptions.Center; text.color = color; }
    static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
}
