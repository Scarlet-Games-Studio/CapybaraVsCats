using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mantém um único anúncio premiado pré-carregado
/// para o Continue da tela de Game Over.
///
/// Os IDs utilizados abaixo são IDs oficiais de teste do Google.
/// </summary>
[DefaultExecutionOrder(-3000)]
public sealed class AdMobRewardedService : MonoBehaviour
{
#if UNITY_ANDROID
    private const string TestRewardedAdUnitId =
        "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
    private const string TestRewardedAdUnitId =
        "ca-app-pub-3940256099942544/1712485313";
#else
    private const string TestRewardedAdUnitId = "unused";
#endif

    private static AdMobRewardedService instance;

    private RewardedAd rewardedAd;

    private Action rewardCallback;
    private Action<string> unavailableCallback;

    private bool initialized;
    private bool loading;
    private bool showing;
    private bool rewardEarned;

    /// <summary>
    /// Retorna true quando existe um anúncio pronto para ser exibido.
    /// No Editor funciona como simulador.
    /// </summary>
    public static bool IsReady
    {
        get
        {
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS
            return true;
#else
            return instance != null &&
                   instance.rewardedAd != null &&
                   instance.rewardedAd.CanShowAd();
#endif
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
            return;

        GameObject serviceObject =
            new GameObject("AdMob Rewarded Service");

        instance =
            serviceObject.AddComponent<AdMobRewardedService>();

        DontDestroyOnLoad(serviceObject);
    }

    private void Start()
    {
#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS

        initialized = true;

        Debug.Log(
            "[AdMob TESTE] Simulador do anúncio premiado pronto no Editor."
        );

#else

        MobileAds.Initialize(_ =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                initialized = true;
                LoadAd();
            });
        });

#endif
    }

    /// <summary>
    /// Solicita a exibição do anúncio usado pelo Continue.
    /// </summary>
    public static bool ShowContinueAd(
        Action onRewardGranted,
        Action<string> onUnavailable)
    {
        if (instance == null)
            Bootstrap();

        return instance != null &&
               instance.Show(
                   onRewardGranted,
                   onUnavailable
               );
    }

    private bool Show(
        Action onRewardGranted,
        Action<string> onUnavailable)
    {
        if (showing)
            return false;

#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS

        ShowEditorTestAd(
            onRewardGranted,
            onUnavailable
        );

        return true;

#else

        if (rewardedAd == null ||
            !rewardedAd.CanShowAd())
        {
            if (initialized && !loading)
                LoadAd();

            onUnavailable?.Invoke(
                "O anúncio de teste ainda não está pronto. " +
                "Tente novamente em alguns segundos."
            );

            return false;
        }

        showing = true;
        rewardEarned = false;

        rewardCallback = onRewardGranted;
        unavailableCallback = onUnavailable;

        rewardedAd.Show(reward =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                rewardEarned = true;

                Debug.Log(
                    $"[AdMob TESTE] Recompensa recebida: " +
                    $"{reward.Amount} {reward.Type}"
                );
            });
        });

        return true;

#endif
    }

    /// <summary>
    /// Carrega um novo anúncio premiado.
    /// </summary>
    private void LoadAd()
    {
        if (!initialized)
            return;

        if (loading)
            return;

        if (showing)
            return;

        loading = true;

        DestroyLoadedAd();

        AdRequest request = new AdRequest();

        RewardedAd.Load(
            TestRewardedAdUnitId,
            request,
            (ad, error) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    loading = false;

                    if (error != null || ad == null)
                    {
                        Debug.LogWarning(
                            "[AdMob TESTE] Falha ao carregar " +
                            $"anúncio premiado: {error}"
                        );

                        return;
                    }

                    rewardedAd = ad;

                    RegisterEvents(ad);

                    Debug.Log(
                        "[AdMob TESTE] Anúncio premiado " +
                        "carregado e pronto."
                    );
                });
            }
        );
    }

    /// <summary>
    /// Registra os eventos do anúncio.
    /// </summary>
    private void RegisterEvents(RewardedAd ad)
    {
        /*
         * CORREÇÃO DO CS1503
         *
         * ExecuteInUpdate espera System.Action.
         *
         * FinishPresentation possui:
         *
         * void FinishPresentation(string failureMessage = null)
         *
         * Portanto não podemos passar simplesmente:
         *
         * ExecuteInUpdate(FinishPresentation);
         *
         * Precisamos criar explicitamente um Action sem parâmetros.
         */

        ad.OnAdFullScreenContentClosed += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                FinishPresentation();
            });
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                string message =
                    $"Não foi possível abrir o anúncio de teste: {error}";

                FinishPresentation(message);
            });
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.Log(
                    "[AdMob TESTE] Anúncio premiado aberto."
                );
            });
        };
    }

    /// <summary>
    /// Finaliza a apresentação do anúncio.
    /// </summary>
    private void FinishPresentation(
        string failureMessage = null)
    {
        if (!showing)
            return;

        bool grantReward =
            rewardEarned &&
            string.IsNullOrEmpty(failureMessage);

        Action reward = rewardCallback;
        Action<string> unavailable = unavailableCallback;

        showing = false;
        rewardEarned = false;

        rewardCallback = null;
        unavailableCallback = null;

        DestroyLoadedAd();

        // Já começa a preparar o próximo anúncio.
        LoadAd();

        if (grantReward)
        {
            Debug.Log(
                "[AdMob TESTE] Recompensa concedida ao jogador."
            );

            reward?.Invoke();
        }
        else
        {
            string message =
                failureMessage ??
                "O anúncio foi fechado antes da recompensa.";

            Debug.LogWarning(
                $"[AdMob TESTE] Recompensa não concedida: {message}"
            );

            unavailable?.Invoke(message);
        }
    }

    /// <summary>
    /// Destrói o anúncio atual.
    /// </summary>
    private void DestroyLoadedAd()
    {
        if (rewardedAd == null)
            return;

        rewardedAd.Destroy();
        rewardedAd = null;
    }

#if UNITY_EDITOR && !UNITY_ANDROID && !UNITY_IOS

    /// <summary>
    /// Simulação de Rewarded Ad dentro do Editor.
    /// </summary>
    private void ShowEditorTestAd(
        Action onRewardGranted,
        Action<string> onUnavailable)
    {
        if (showing)
            return;

        showing = true;

        GameObject root = new GameObject(
            "AdMob Test Rewarded Preview",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        DontDestroyOnLoad(root);

        Canvas canvas =
            root.GetComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder =
            short.MaxValue;

        CanvasScaler scaler =
            root.GetComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920f, 1080f);

        Image background = CreateImage(
            "Background",
            root.transform,
            new Color(
                0.015f,
                0.02f,
                0.055f,
                0.99f
            )
        );

        Stretch(background.rectTransform);

        CreateText(
            "Title",
            background.transform,
            "ANÚNCIO PREMIADO DE TESTE",
            52f,
            new Vector2(0f, 120f)
        );

        CreateText(
            "Info",
            background.transform,
            "Simulação do Editor — a build Android usa " +
            "o anúncio oficial de teste do Google.",
            24f,
            new Vector2(0f, 42f)
        );

        CreateButton(
            "Earn Reward",
            background.transform,
            "CONCLUIR E CONTINUAR",
            new Vector2(0f, -75f),
            () =>
            {
                showing = false;

                Destroy(root);

                Debug.Log(
                    "[AdMob TESTE] Recompensa simulada concedida."
                );

                onRewardGranted?.Invoke();
            }
        );

        CreateButton(
            "Close",
            background.transform,
            "FECHAR SEM RECOMPENSA",
            new Vector2(0f, -190f),
            () =>
            {
                showing = false;

                Destroy(root);

                onUnavailable?.Invoke(
                    "O anúncio de teste foi fechado " +
                    "antes da recompensa."
                );
            }
        );
    }

    private static Image CreateImage(
        string name,
        Transform parent,
        Color color)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image)
        );

        go.transform.SetParent(
            parent,
            false
        );

        Image image =
            go.GetComponent<Image>();

        image.color = color;

        return image;
    }

    private static void CreateText(
        string name,
        Transform parent,
        string value,
        float size,
        Vector2 position)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        go.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            (RectTransform)go.transform;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(1500f, 90f);

        rect.anchoredPosition =
            position;

        TextMeshProUGUI text =
            go.GetComponent<TextMeshProUGUI>();

        text.text = value;
        text.fontSize = size;
        text.alignment =
            TextAlignmentOptions.Center;

        text.color = Color.white;
    }

    private static void CreateButton(
        string name,
        Transform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Image image = CreateImage(
            name,
            parent,
            new Color(
                0.02f,
                0.38f,
                0.72f,
                1f
            )
        );

        RectTransform rect =
            image.rectTransform;

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.sizeDelta =
            new Vector2(540f, 82f);

        rect.anchoredPosition =
            position;

        Button button =
            image.gameObject.AddComponent<Button>();

        button.targetGraphic = image;

        button.onClick.AddListener(action);

        CreateText(
            "Label",
            image.transform,
            label,
            24f,
            Vector2.zero
        );
    }

    private static void Stretch(
        RectTransform rect)
    {
        rect.anchorMin =
            Vector2.zero;

        rect.anchorMax =
            Vector2.one;

        rect.offsetMin =
            Vector2.zero;

        rect.offsetMax =
            Vector2.zero;
    }

#endif

    private void OnDestroy()
    {
        DestroyLoadedAd();

        if (instance == this)
            instance = null;
    }
}