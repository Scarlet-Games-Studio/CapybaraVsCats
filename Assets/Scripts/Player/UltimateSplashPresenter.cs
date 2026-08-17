using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UltimateSplashPresenter
{
    public static void Show(Sprite splashSprite, float totalDuration)
    {
        if (splashSprite == null) return;

        GameObject root = new("Mika Ultimate Splash", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1366f, 768f);
        scaler.matchWidthOrHeight = 1f;

        CanvasGroup group = root.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        GameObject flashObject = new("Pink Flash", typeof(RectTransform), typeof(Image));
        flashObject.transform.SetParent(root.transform, false);
        RectTransform flashRect = flashObject.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = flashRect.offsetMax = Vector2.zero;
        Image flash = flashObject.GetComponent<Image>();
        flash.color = new Color(1f, 0.12f, 0.65f, 0f);
        flash.raycastTarget = false;

        GameObject artObject = new("Mika Splash Art", typeof(RectTransform), typeof(Image));
        artObject.transform.SetParent(root.transform, false);
        RectTransform artRect = artObject.GetComponent<RectTransform>();
        artRect.anchorMin = artRect.anchorMax = new Vector2(0f, 0.5f);
        artRect.pivot = new Vector2(0.5f, 0.5f);
        artRect.sizeDelta = new Vector2(1024f, 768f);
        // A textura tem a ilustração no lado esquerdo e transparência à direita.
        // Mantém a arte próxima da borda sem cobrir a área central do combate.
        artRect.anchoredPosition = new Vector2(-620f, 0f);
        artRect.localScale = new Vector3(0.88f, 0.88f, 1f);
        artRect.localRotation = Quaternion.Euler(0f, 0f, -2.5f);

        Image art = artObject.GetComponent<Image>();
        art.sprite = splashSprite;
        art.preserveAspect = true;
        art.raycastTarget = false;

        float holdDuration = Mathf.Max(0.15f, totalDuration - 0.9f);
        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetLink(root);
        sequence.Append(group.DOFade(1f, 0.16f));
        sequence.Join(flash.DOFade(0.2f, 0.1f).SetLoops(2, LoopType.Yoyo));
        sequence.Join(artRect.DOAnchorPosX(283f, 0.38f).SetEase(Ease.OutBack, 1.15f));
        sequence.Join(artRect.DOScale(1f, 0.42f).SetEase(Ease.OutCubic));
        sequence.Join(artRect.DORotate(Vector3.zero, 0.38f).SetEase(Ease.OutCubic));
        sequence.Append(artRect.DOPunchScale(Vector3.one * 0.035f, 0.28f, 5, 0.45f));
        sequence.AppendInterval(holdDuration);
        sequence.Append(artRect.DOAnchorPosX(-620f, 0.32f).SetEase(Ease.InBack));
        sequence.Join(group.DOFade(0f, 0.25f));
        sequence.OnComplete(() => Object.Destroy(root));
    }
}
