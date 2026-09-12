using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartCountdown : MonoBehaviour
{
    public RawImage readyImage;
    public RawImage goImage;
    [Min(0.1f)] public float displayTime = 0.8f;
    [Min(0.05f)] public float fadeDuration = 0.25f;

    Sequence countdownSequence;
    bool countdownRunning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ActivateConfiguredCountdown()
    {
        GameStartCountdown countdown = FindAnyObjectByType<GameStartCountdown>(FindObjectsInactive.Include);
        if (countdown == null || countdown.gameObject.scene != SceneManager.GetActiveScene()) return;

        // Garante a ativação mesmo se algum pai tiver sido salvo desativado.
        Transform current = countdown.transform;
        while (current != null)
        {
            current.gameObject.SetActive(true);
            current = current.parent;
        }
    }

    void Start()
    {
        if (readyImage == null || goImage == null)
        {
            Debug.LogWarning("Countdown sem imagens configuradas; iniciando o jogo sem bloqueio.", this);
            ResumeGameplay();
            enabled = false;
            return;
        }

        PrepareOverlayCanvas();
        PrepareImage(readyImage);
        PrepareImage(goImage);
        PlayCountdown();
    }

    void PrepareOverlayCanvas()
    {
        // A UI original está com escala zero na cena. Separar somente o READY/GO
        // evita alterar o HUD/analógicos que o usuário já montou.
        transform.SetParent(null, false);
        RectTransform root = transform as RectTransform;
        if (root != null)
        {
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.localScale = Vector3.one;
            root.anchoredPosition = Vector2.zero;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    void PrepareImage(RawImage image)
    {
        image.raycastTarget = false;
        image.gameObject.SetActive(false);
        Color color = image.color;
        color.a = 0f;
        image.color = color;
    }

    void PlayCountdown()
    {
        countdownSequence?.Kill();
        countdownRunning = true;
        Time.timeScale = 0f;

        Vector3 readyScale = readyImage.rectTransform.localScale;
        Vector3 goScale = goImage.rectTransform.localScale;

        countdownSequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
        AppendCard(countdownSequence, readyImage, readyScale);
        countdownSequence.AppendInterval(0.12f);
        AppendCard(countdownSequence, goImage, goScale);
        countdownSequence.OnComplete(() =>
        {
            countdownRunning = false;
            ResumeGameplay();
            gameObject.SetActive(false);
        });
    }

    void AppendCard(Sequence sequence, RawImage image, Vector3 targetScale)
    {
        sequence.AppendCallback(() =>
        {
            image.gameObject.SetActive(true);
            image.rectTransform.localScale = targetScale * 0.72f;
        });
        sequence.Append(image.DOFade(1f, fadeDuration));
        sequence.Join(image.rectTransform.DOScale(targetScale, fadeDuration + 0.08f).SetEase(Ease.OutBack, 1.25f));
        sequence.AppendInterval(displayTime);
        sequence.Append(image.DOFade(0f, fadeDuration));
        sequence.Join(image.rectTransform.DOScale(targetScale * 1.08f, fadeDuration).SetEase(Ease.InCubic));
        sequence.AppendCallback(() =>
        {
            image.gameObject.SetActive(false);
            image.rectTransform.localScale = targetScale;
        });
    }

    void ResumeGameplay()
    {
        if (GameManager.instance == null || GameManager.instance.IsPlaying)
            Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        countdownSequence?.Kill();
        if (countdownRunning) ResumeGameplay();
    }
}
