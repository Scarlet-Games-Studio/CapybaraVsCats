using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    }

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
