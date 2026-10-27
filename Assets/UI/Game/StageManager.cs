using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [Tooltip("Após concluir o Stage 1, o jogo exibe a tela de conteúdo em breve.")]
    public string comingSoonSceneName = "ComingSoon";
    public string mapSceneName = "Map";
    public string lobbySceneName = "Lobby";
    public string mainMenuSceneName = "MainMenu";

    [Header("Pontuação")]
    [Min(1)] public int maxScore = 10000;
    [SerializeField, Range(0f, 1f)] float twoStarScoreRatio = 0.5f;
    [SerializeField, Range(0f, 1f)] float threeStarScoreRatio = 0.8f;

    bool completed;
    bool transitioning;
    Sequence panelSequence;
    Sequence starsSequence;
    Graphic[] starGraphics = Array.Empty<Graphic>();
    readonly Dictionary<Graphic, Vector3> starBaseScales = new();

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
        ConfigureButtonMotion(nextButton, 0.24f, true);
        ConfigureButtonMotion(mapButton, 0.32f, false);
        ConfigureButtonMotion(lobbyButton, 0.4f, false);
        ConfigureButtonMotion(exitButton, 0.48f, false);
        ResolveStars();
    }

    void OnDestroy()
    {
        panelSequence?.Kill();
        starsSequence?.Kill();
        if (nextButton != null) nextButton.onClick.RemoveListener(GoToNextStage);
    }

    static void ConfigureButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    static void ConfigureButtonMotion(Button button, float delay, bool pulse)
    {
        if (button == null) return;
        MenuButtonMotion motion = button.GetComponent<MenuButtonMotion>();
        if (motion == null) motion = button.gameObject.AddComponent<MenuButtonMotion>();
        motion.Configure(delay, pulse);
    }

    public void OnStageComplete()
    {
        if (completed) return;
        completed = true;

        if (stageCompleteUI == null)
        {
            Debug.LogWarning("A UI de conclusão da fase não está configurada.", this);
            Time.timeScale = 0f;
            return;
        }

        ShowStageCompleteUI();
        ResolveStars();
        if (scoreViewText != null) scoreViewText.text = ScoreManager.score.ToString("N0");
        int earnedStars = UpdateStars(ScoreManager.score, maxScore);
        PlayPanelEntrance();
        // Evita que o toque/tiro que derrotou o boss atravesse o painel e
        // pressione um botão no mesmo frame em que ele apareceu.
        SetNavigationInteractable(false);
        Time.timeScale = 0f;
        StartCoroutine(EnableNavigationAfterInputRelease());

        try
        {
            ProgressManager.SaveProgress();
            ProgressManager.SaveStageScore(ScoreManager.score);
            ProgressManager.SaveStageStars(earnedStars);
        }
        catch (Exception exception)
        {
            // A conclusão e seus botões continuam utilizáveis mesmo se o salvamento local falhar.
            Debug.LogException(exception, this);
        }
    }

    void ShowStageCompleteUI()
    {
        Canvas canvas = stageCompleteUI.GetComponentInParent<Canvas>(true);
        Transform current = stageCompleteUI.transform;
        Transform topLevelPanel = current;

        while (current != null && (canvas == null || current != canvas.transform))
        {
            current.gameObject.SetActive(true);
            topLevelPanel = current;

            CanvasGroup group = current.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }

            current = current.parent;
        }

        if (canvas != null) canvas.gameObject.SetActive(true);
        if (topLevelPanel.parent != null) topLevelPanel.SetAsLastSibling();
        stageCompleteUI.transform.SetAsLastSibling();
    }

    void PlayPanelEntrance()
    {
        panelSequence?.Kill();
        CanvasGroup group = stageCompleteUI.GetComponent<CanvasGroup>();
        if (group == null) group = stageCompleteUI.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        Transform card = stageCompleteUI.transform.Find("Card");
        if (card == null && stageCompleteUI.transform.childCount > 0)
            card = stageCompleteUI.transform.GetChild(0);

        Vector3 cardScale = card != null ? card.localScale : Vector3.one;
        if (card != null) card.localScale = cardScale * 0.82f;

        panelSequence = DOTween.Sequence().SetUpdate(true).SetLink(stageCompleteUI);
        panelSequence.Append(group.DOFade(1f, 0.22f));
        if (card != null)
        {
            panelSequence.Join(card.DOScale(cardScale, 0.4f).SetEase(Ease.OutBack, 1.18f));
            panelSequence.Append(card.DOPunchScale(cardScale * 0.018f, 0.18f, 4, 0.3f));
        }
    }

    IEnumerator EnableNavigationAfterInputRelease()
    {
        yield return new WaitForSecondsRealtime(0.35f);

        float timeout = Time.realtimeSinceStartup + 1.5f;
        while ((Input.GetMouseButton(0) || Input.touchCount > 0) && Time.realtimeSinceStartup < timeout)
            yield return null;

        if (!transitioning && completed)
            SetNavigationInteractable(true);
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

        string destination = ResolveNextScene();
        LoadScene(destination);
    }

    string ResolveNextScene()
    {
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

    int UpdateStars(int score, int possibleScore)
    {
        float ratio = possibleScore > 0 ? Mathf.Clamp01((float)score / possibleScore) : 0f;
        int earnedStars = score <= 0 ? 0 : ratio >= Mathf.Max(twoStarScoreRatio, threeStarScoreRatio)
            ? 3
            : ratio >= Mathf.Min(twoStarScoreRatio, threeStarScoreRatio) ? 2 : 1;

        if (starFill != null) starFill.fillAmount = earnedStars / 3f;
        if (starsValueText != null) starsValueText.text = $"{earnedStars} / 3 ESTRELAS";
        AnimateStars(earnedStars);
        return earnedStars;
    }

    void ResolveStars()
    {
        starBaseScales.Clear();
        if (stars == null)
        {
            starGraphics = Array.Empty<Graphic>();
            return;
        }

        var found = new List<Graphic>();
        foreach (Graphic graphic in stars.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.name.StartsWith("star", StringComparison.OrdinalIgnoreCase))
                found.Add(graphic);
        }

        found.Sort((left, right) => string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase));
        if (found.Count > 3) found.RemoveRange(3, found.Count - 3);
        starGraphics = found.ToArray();
        foreach (Graphic graphic in starGraphics) starBaseScales[graphic] = graphic.transform.localScale;
    }

    void AnimateStars(int earnedStars)
    {
        if (starGraphics.Length == 0) ResolveStars();
        starsSequence?.Kill();
        starsSequence = DOTween.Sequence().SetUpdate(true).SetLink(stageCompleteUI);

        Color lockedColor = new Color(0.2f, 0.25f, 0.34f, 0.38f);
        Color earnedColor = new Color(1f, 0.78f, 0.2f, 1f);
        for (int i = 0; i < starGraphics.Length; i++)
        {
            Graphic star = starGraphics[i];
            star.transform.DOKill();
            star.DOKill();
            Vector3 baseScale = starBaseScales.TryGetValue(star, out Vector3 savedScale)
                ? savedScale
                : star.transform.localScale;

            if (i >= earnedStars)
            {
                star.color = lockedColor;
                star.transform.localScale = baseScale;
                continue;
            }

            star.color = new Color(earnedColor.r, earnedColor.g, earnedColor.b, 0f);
            star.transform.localScale = baseScale * 0.15f;
            float revealAt = 0.34f + i * 0.2f;
            starsSequence.Insert(revealAt, star.DOColor(earnedColor, 0.18f));
            starsSequence.Insert(revealAt,
                star.transform.DOScale(baseScale, 0.38f).SetEase(Ease.OutBack, 1.55f));
            starsSequence.Insert(revealAt + 0.34f,
                star.transform.DOPunchScale(baseScale * 0.12f, 0.2f, 5, 0.35f));
        }
    }
}
