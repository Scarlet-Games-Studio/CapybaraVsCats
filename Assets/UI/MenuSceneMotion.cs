using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Small, reusable motion cues for the non-gameplay menus.</summary>
public static class MenuSceneMotion
{
    public static void Enter(RectTransform rect, float delay, Vector2 offset, float scaleFrom = 0.94f)
    {
        if (rect == null) return;

        rect.DOKill();
        CanvasGroup group = rect.GetComponent<CanvasGroup>();
        if (group == null) group = rect.gameObject.AddComponent<CanvasGroup>();

        Vector2 targetPosition = rect.anchoredPosition;
        Vector3 targetScale = rect.localScale;
        rect.anchoredPosition = targetPosition + offset;
        rect.localScale = targetScale * scaleFrom;
        group.alpha = 0f;

        Sequence entrance = DOTween.Sequence().SetUpdate(true).SetLink(rect.gameObject);
        entrance.SetDelay(delay);
        entrance.Join(rect.DOAnchorPos(targetPosition, 0.52f).SetEase(Ease.OutCubic));
        entrance.Join(rect.DOScale(targetScale, 0.58f).SetEase(Ease.OutBack));
        entrance.Join(group.DOFade(1f, 0.34f));
    }

    public static void Float(RectTransform rect, float distance = 7f, float duration = 2.6f, float delay = 0f)
    {
        if (rect == null) return;
        rect.DOKill();
        rect.DOAnchorPosY(rect.anchoredPosition.y + distance, duration)
            .SetDelay(delay)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .SetLink(rect.gameObject);
    }

    public static void PrepareButton(Button button, float delay, bool emphasize = false)
    {
        if (button == null) return;
        Enter(button.transform as RectTransform, delay, new Vector2(0f, -18f), emphasize ? 0.88f : 0.96f);

        // Unity's selectable transitions provide a clear, low-cost response to both touch and mouse.
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.fadeDuration = 0.12f;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.72f, 0.9f, 1f, 1f);
        colors.selectedColor = new Color(0.9f, 0.97f, 1f, 1f);
        button.colors = colors;

    }

    public static void Cancel(Transform root)
    {
        if (root == null) return;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            child.DOKill();
            CanvasGroup group = child.GetComponent<CanvasGroup>();
            if (group != null) group.DOKill();
        }
    }
}
