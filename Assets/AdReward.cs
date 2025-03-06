using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class AdReward : MonoBehaviour
{
    public Button rewardedAdButton; // Botão que exibe o anúncio
    public GameObject gameOverPanel; // Painel de Game Over

    private RewardedAd rewardedAd;
    private string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // ID de TESTE

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob Inicializado!");
            LoadRewardedAd();
        });

        if (rewardedAdButton != null)
        {
            rewardedAdButton.onClick.AddListener(ShowRewardedAd);
        }
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("Carregando anúncio recompensado...");

        AdRequest adRequest = new AdRequest();

        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Falha ao carregar o anúncio recompensado: " + error);
                return;
            }

            Debug.Log("Anúncio recompensado carregado!");
            rewardedAd = ad;

            // Eventos do anúncio
            rewardedAd.OnAdFullScreenContentClosed += HandleAdClosed;
            rewardedAd.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                Debug.LogError("Erro ao exibir anúncio: " + adError);
            };
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(HandleUserEarnedReward);
        }
        else
        {
            Debug.Log("Anúncio ainda não carregado!");
        }
    }

    private void HandleUserEarnedReward(Reward reward)
    {
        Debug.Log("Usuário assistiu ao anúncio! Reiniciando o jogo...");

        // Esconde o painel de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Despausa o jogo
        Time.timeScale = 1;

        // Reinicia a cena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandleAdClosed()
    {
        Debug.Log("Anúncio fechado. Carregando novo anúncio...");
        LoadRewardedAd();
    }
}
