using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    public enum RankingScope { Mundial, Regional, Local }

    [Header("Perfil")]
    public Image characterSplash;
    public TMP_Text characterNameText;
    public TMP_Text bestScoreText;
    public Sprite hiroSplash;
    public Sprite mikaSplash;
    public Sprite edgeSplash;

    [Header("Ranking")]
    public TMP_Text rankingTitleText;
    public TMP_Text rankingStatusText;
    public TMP_Text[] rankingRows;
    public Button worldButton;
    public Button regionalButton;
    public Button localButton;

    [Header("Navegação")]
    public Button mapButton;
    public Button exitButton;
    public string mapSceneName = "Map";
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        ShowMostUsedCharacter();
        worldButton?.onClick.AddListener(() => ShowRanking(RankingScope.Mundial));
        regionalButton?.onClick.AddListener(() => ShowRanking(RankingScope.Regional));
        localButton?.onClick.AddListener(() => ShowRanking(RankingScope.Local));
        mapButton?.onClick.AddListener(() => Load(mapSceneName));
        exitButton?.onClick.AddListener(() => Load(mainMenuSceneName));
        ShowRanking(RankingScope.Local);
        AnimateLobby();
    }

    void AnimateLobby()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransform header = FindRect(canvasRect, "Header");
        RectTransform logo = FindRect(canvasRect, "Capybara vs Cats Logo");
        RectTransform profile = FindRect(canvasRect, "Most Used Character");
        RectTransform ranking = FindRect(canvasRect, "Ranking Panel And Title");
        MenuSceneMotion.Enter(header, 0.05f, new Vector2(0f, 22f), 0.97f);
        MenuSceneMotion.Enter(logo, 0.1f, new Vector2(0f, 20f), 0.92f);
        MenuSceneMotion.Enter(profile, 0.24f, new Vector2(-28f, 0f), 0.93f);
        MenuSceneMotion.Enter(ranking, 0.3f, new Vector2(28f, 0f), 0.97f);
        MenuSceneMotion.Enter(characterNameText != null ? characterNameText.rectTransform : null, 0.3f, new Vector2(0f, -10f), 0.98f);
        MenuSceneMotion.Enter(bestScoreText != null ? bestScoreText.rectTransform : null, 0.36f, new Vector2(0f, -10f), 0.98f);
        MenuSceneMotion.Float(characterSplash != null ? characterSplash.rectTransform : null, 6f, 2.4f, 0.5f);

        if (rankingRows != null)
            for (int i = 0; i < rankingRows.Length; i++)
                MenuSceneMotion.Enter(rankingRows[i] != null ? rankingRows[i].rectTransform : null, 0.42f + i * 0.035f, new Vector2(12f, 0f), 0.99f);

        MenuSceneMotion.PrepareButton(worldButton, 0.36f);
        MenuSceneMotion.PrepareButton(regionalButton, 0.42f);
        MenuSceneMotion.PrepareButton(localButton, 0.48f, true);
        MenuSceneMotion.PrepareButton(mapButton, 0.54f, true);
        MenuSceneMotion.PrepareButton(exitButton, 0.6f);
    }

    static RectTransform FindRect(Transform root, string objectName)
    {
        if (root == null) return null;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == objectName) return child as RectTransform;
        return null;
    }

    void OnDisable() => MenuSceneMotion.Cancel(transform);

    void ShowMostUsedCharacter()
    {
        CharacterSelection.Character character = CharacterSelection.MostUsed;
        if (characterSplash != null)
            characterSplash.sprite = character == CharacterSelection.Character.Mika ? mikaSplash : character == CharacterSelection.Character.Edge ? edgeSplash : hiroSplash;
        if (characterNameText != null)
            characterNameText.text = character == CharacterSelection.Character.Mika ? "MIKA — GAROTA MÁGICA" : character == CharacterSelection.Character.Edge ? "EDGE" : "HIRO";
        if (bestScoreText != null) bestScoreText.text = $"RECORDE  {ProgressManager.GetBestScore():N0}";
    }

    public void ShowRanking(RankingScope scope)
    {
        if (rankingTitleText != null) rankingTitleText.text = $"RANKING {scope.ToString().ToUpperInvariant()}";
        bool online = scope != RankingScope.Local;
        if (rankingStatusText != null)
            rankingStatusText.text = online ? "SERVIÇO ONLINE EM BREVE • SEU RECORDE LOCAL" : "RECORDES SALVOS NESTE DISPOSITIVO";

        int[] scores = ProgressManager.GetLocalRanking();
        for (int i = 0; i < rankingRows.Length; i++)
        {
            if (rankingRows[i] == null) continue;
            int score = i < scores.Length ? scores[i] : 0;
            if (online)
                rankingRows[i].text = i == 0 ? $"—  VOCÊ     {ProgressManager.GetBestScore():N0}" : "—  AGUARDANDO SERVIDOR";
            else
                rankingRows[i].text = score > 0 ? $"{i + 1:00}  JOGADOR     {score:N0}" : $"{i + 1:00}  ---          0";
        }
    }

    static void Load(string scene)
    {
        if (Application.CanStreamedLevelBeLoaded(scene)) SceneManager.LoadScene(scene);
        else Debug.LogError($"Cena '{scene}' não está habilitada no Build Settings.");
    }
}
