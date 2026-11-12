using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MapSceneController : MonoBehaviour
{
    [Header("Planetas disponíveis")]
    public Button earthButton;
    public Button marsButton;
    [Header("Navegação")]
    public Button lobbyButton;
    public Button exitButton;
    public Button[] comingSoonButtons;

    [Header("Cenas")]
    public string earthScene = "ingame";
    public string marsScene = "ComingSoon";
    public string comingSoonScene = "ComingSoon";
    public string lobbyScene = "Lobby";
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        Bind(earthButton, earthScene);
        Bind(marsButton, marsScene);
        Bind(lobbyButton, lobbyScene);
        Bind(exitButton, mainMenuScene);
        if (comingSoonButtons != null)
            foreach (Button button in comingSoonButtons) Bind(button, comingSoonScene);

        AnimateMap();
    }

    void AnimateMap()
    {
        MenuSceneMotion.PrepareButton(earthButton, 0.18f, true);
        MenuSceneMotion.PrepareButton(marsButton, 0.34f);
        MenuSceneMotion.PrepareButton(lobbyButton, 0.48f);
        MenuSceneMotion.PrepareButton(exitButton, 0.56f);

        if (comingSoonButtons != null)
            for (int i = 0; i < comingSoonButtons.Length; i++)
                MenuSceneMotion.PrepareButton(comingSoonButtons[i], 0.28f + i * 0.07f);

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform[] accents = canvas.GetComponentsInChildren<RectTransform>(true);
        int glowIndex = 0;
        foreach (RectTransform accent in accents)
        {
            if (accent.name == "Glow")
            {
                accent.DOKill();
                Vector3 baseScale = accent.localScale;
                accent.DOScale(baseScale * 1.13f, 1.8f + glowIndex * 0.16f)
                    .SetDelay(glowIndex * 0.12f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true)
                    .SetLink(accent.gameObject);
                glowIndex++;
            }
        }
    }

    void OnDisable() => MenuSceneMotion.Cancel(transform);

    static void Bind(Button button, string scene)
    {
        if (button == null || string.IsNullOrWhiteSpace(scene)) return;
        button.onClick.AddListener(() =>
        {
            if (Application.CanStreamedLevelBeLoaded(scene)) SceneManager.LoadScene(scene);
            else Debug.LogError($"Cena '{scene}' não está habilitada no Build Settings.");
        });
    }
}
