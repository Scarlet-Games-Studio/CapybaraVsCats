using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class RewardedAdManager : MonoBehaviour
{
    private RewardedAd rewardedAd;
    private string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // ID de teste

    void Start()
    {
        MobileAds.Initialize(initStatus => { LoadRewardedAd(); });
    }

    public void LoadRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest adRequest = new AdRequest();

        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Falha ao carregar o anúncio: " + error);
                return;
            }

            rewardedAd = ad;
            RegisterEventHandlers(rewardedAd);
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(reward =>
            {
                Debug.Log("Usuário recompensado. Reiniciando o jogo...");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
        else
        {
            Debug.Log("O anúncio ainda não está pronto.");
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Anúncio fechado. Recarregando...");
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Falha ao exibir o anúncio: " + error);
            LoadRewardedAd();
        };
    }
}
