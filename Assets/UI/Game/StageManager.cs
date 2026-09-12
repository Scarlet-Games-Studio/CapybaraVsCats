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
    [Tooltip("Após concluir o Stage 1, o jogo exibe a tela de conteúdo em breve.")]
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

        if (stageCompleteUI == null)
        {
            Debug.LogWarning("A UI de conclusão da fase não está configurada.", this);
            Time.timeScale = 0f;
            return;
        }

        ShowStageCompleteUI();
        if (scoreViewText != null) scoreViewText.text = $"SCORE  {ScoreManager.score:N0}";
        UpdateStars(ScoreManager.score, maxScore);
        // Evita que o toque/tiro que derrotou o boss atravesse o painel e
        // pressione um botão no mesmo frame em que ele apareceu.
        SetNavigationInteractable(false);
        Time.timeScale = 0f;
        StartCoroutine(EnableNavigationAfterInputRelease());

        try
        {
            ProgressManager.SaveProgress();
            ProgressManager.SaveStageScore(ScoreManager.score);
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

    void UpdateStars(int score, int possibleScore)
    {
        float ratio = possibleScore > 0 ? Mathf.Clamp01((float)score / possibleScore) : 0f;
        float earnedStars = Mathf.Round(ratio * 6f) * 0.5f;
        if (score > 0) earnedStars = Mathf.Max(0.5f, earnedStars);
        if (starFill != null) starFill.fillAmount = earnedStars / 3f;
        if (starsValueText != null) starsValueText.text = $"{earnedStars:0.#} / 3 ESTRELAS";
    }
}
