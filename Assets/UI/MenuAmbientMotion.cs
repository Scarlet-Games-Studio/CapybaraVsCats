using DG.Tweening;
using UnityEngine;

public class MenuAmbientMotion : MonoBehaviour
{
    [SerializeField] float distance = 12f;
    [SerializeField] float duration = 2.2f;
    RectTransform rect;

    void OnEnable()
    {
        rect = transform as RectTransform;
        if (rect == null) return;
        rect.DOKill();
        rect.DOAnchorPosY(rect.anchoredPosition.y + distance, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    void OnDisable() { if (rect != null) rect.DOKill(); }
}
