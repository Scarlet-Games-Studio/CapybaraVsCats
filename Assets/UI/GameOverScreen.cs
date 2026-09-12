using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public GameObject gameOverUI;
    public GameObject interfacePanel;
    public Button exitButton;
    public Button restartButton;
    public TMP_Text finalScoreText;
    public TMP_Text adStatusText;

    bool shown;
    bool waitingForAd;
    Sequence showSequence;

    void Start()
    {
        ResolveReferences();
        shown = false;
        if (gameOverUI != null) gameOverUI.SetActive(false);
        SetGameplayInterfaceVisible(true);
        BindButtons();
    }

    void ResolveReferences()
    {
        if (gameOverUI == null) return;

        Button[] buttons = gameOverUI.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            string id = button.name.ToLowerInvariant();
            if (restartButton == null && (id.Contains("restart") || id.Contains("reiniciar"))) restartButton = button;
            if (exitButton == null && (id.Contains("exit") || id.Contains("sair"))) exitButton = button;
        }

        if (finalScoreText == null)
        {
            TMP_Text[] texts = gameOverUI.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                string id = text.name.ToLowerInvariant();
                if (!id.Contains("score number") && !id.Contains("pontuacao") && !id.Contains("pontuação")) continue;
                finalScoreText = text;
                break;
            }
        }

        if (adStatusText == null && restartButton != null)
            adStatusText = CreateAdStatusText(restartButton.transform.parent);
    }

    static TMP_Text CreateAdStatusText(Transform parent)
    {
        if (parent == null) return null;
        Transform existing = parent.Find("Ad Status");
        if (existing != null) return existing.GetComponent<TMP_Text>();

        var statusObject = new GameObject("Ad Status", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusObject.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)statusObject.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(720f, 55f);
        rect.anchoredPosition = new Vector2(0f, -218f);
        TextMeshProUGUI text = statusObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = 20f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(.45f, .92f, 1f);
        return text;
    }

    void BindButtons()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitInGame);
            exitButton.onClick.AddListener(ExitInGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RequestContinueWithAd);
            restartButton.onClick.AddListener(RequestContinueWithAd);
        }
    }

    void OnDestroy()
    {
        showSequence?.Kill();
        if (exitButton != null) exitButton.onClick.RemoveListener(ExitInGame);
        if (restartButton != null) restartButton.onClick.RemoveListener(RequestContinueWithAd);
    }

    public void ShowGameOverScreen()
    {
        if (shown) return;
        shown = true;
        waitingForAd = false;
        ResolveReferences();
        BindButtons();

        if (gameOverUI == null)
        {
            Time.timeScale = 0f;
            Debug.LogWarning("Painel de Game Over não está configurado.", this);
            return;
        }

        gameOverUI.SetActive(true);
        gameOverUI.transform.SetAsLastSibling();

        if (finalScoreText != null)
            finalScoreText.text = ScoreManager.score.ToString("N0");

        if (restartButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(restartButton.gameObject);

        if (restartButton != null) restartButton.interactable = true;
        SetAdStatus(AdMobRewardedService.IsReady
            ? "ASSISTA AO ANÚNCIO DE TESTE PARA CONTINUAR"
            : "CARREGANDO ANÚNCIO DE TESTE...");

        PlayEntrance();
        SetGameplayInterfaceVisible(false);
        Time.timeScale = 0f;
    }

    void PlayEntrance()
    {
        showSequence?.Kill();
        CanvasGroup group = gameOverUI.GetComponent<CanvasGroup>();
        if (group == null) group = gameOverUI.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = true;
        group.blocksRaycasts = true;

        Transform card = gameOverUI.transform.Find("Card");
        if (card == null && gameOverUI.transform.childCount > 0) card = gameOverUI.transform.GetChild(0);
        Vector3 targetScale = card != null ? card.localScale : Vector3.one;
        if (card != null) card.localScale = targetScale * 0.78f;

        showSequence = DOTween.Sequence().SetUpdate(true).SetLink(gameOverUI);
        showSequence.Append(group.DOFade(1f, 0.22f));
        if (card != null)
        {
            showSequence.Join(card.DOScale(targetScale, 0.35f).SetEase(Ease.OutBack, 1.15f));
            showSequence.Append(card.DOPunchScale(targetScale * 0.025f, 0.2f, 4, 0.35f));
        }
    }

    void SetGameplayInterfaceVisible(bool visible)
    {
        if (interfacePanel == null || interfacePanel == gameOverUI) return;
        if (gameOverUI != null && gameOverUI.transform.IsChildOf(interfacePanel.transform)) return;
        interfacePanel.SetActive(visible);
    }

    public void RestartGame()
    {
        if (!shown) return;
        shown = false;
        Time.timeScale = 1f;
        FMODManager.ExitPausedState();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RequestContinueWithAd()
    {
        if (!shown || waitingForAd) return;
        waitingForAd = true;
        if (restartButton != null) restartButton.interactable = false;
        SetAdStatus("ABRINDO ANÚNCIO DE TESTE...");

        bool started = AdMobRewardedService.ShowContinueAd(ContinueGame, HandleAdUnavailable);
        if (!started && waitingForAd)
            HandleAdUnavailable("O anúncio de teste ainda não está pronto. Tente novamente.");
    }

    void ContinueGame()
    {
        if (!shown) return;
        Health playerHealth = FindPlayerHealth();
        if (playerHealth == null || !playerHealth.RevivePlayer())
        {
            Debug.LogWarning("Player não encontrado para continuar; reiniciando a fase.", this);
            RestartGame();
            return;
        }

        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("EnemyProjectile"))
        {
            projectile.SetActive(false);
            Destroy(projectile);
        }

        shown = false;
        waitingForAd = false;
        showSequence?.Kill();
        if (gameOverUI != null) gameOverUI.SetActive(false);
        SetGameplayInterfaceVisible(true);
        if (GameManager.instance != null) GameManager.instance.ContinueAfterReward();
        else
        {
            Time.timeScale = 1f;
            FMODManager.ExitPausedState();
        }
    }

    static Health FindPlayerHealth()
    {
        Health[] healthComponents = FindObjectsByType<Health>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Health health in healthComponents)
            if (health != null && health.CompareTag("Player")) return health;
        return null;
    }

    void HandleAdUnavailable(string message)
    {
        waitingForAd = false;
        if (restartButton != null) restartButton.interactable = true;
        SetAdStatus(message);
        Debug.LogWarning($"[AdMob TESTE] {message}", this);
    }

    void SetAdStatus(string message)
    {
        if (adStatusText != null) adStatusText.text = message;
    }

    public void ExitInGame()
    {
        if (!shown) return;
        shown = false;
        Time.timeScale = 1f;
        FMODManager.ExitPausedState();
        if (gameOverUI != null) gameOverUI.SetActive(false);
        SetGameplayInterfaceVisible(true);

        const string mainMenu = "MainMenu";
        if (Application.CanStreamedLevelBeLoaded(mainMenu)) SceneManager.LoadScene(mainMenu);
        else Debug.LogError($"A cena '{mainMenu}' não está habilitada no Build Settings.");
    }
}
