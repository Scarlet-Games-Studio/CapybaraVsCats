using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float entranceDelay;
    [SerializeField] float hoverScale = 1.06f;
    [SerializeField] float pressedScale = .93f;
    [SerializeField] bool pulse;
    Vector3 baseScale;
    Tween pulseTween;

    public void Configure(float delay, bool usePulse)
    {
        entranceDelay = delay;
        pulse = usePulse;
    }

    void OnEnable()
    {
        baseScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.DOScale(baseScale, .48f).SetDelay(entranceDelay).SetEase(Ease.OutBack).SetUpdate(true).OnComplete(StartPulse);
    }

    void OnDisable() { transform.DOKill(); pulseTween?.Kill(); }

    void StartPulse()
    {
        if (!pulse) return;
        pulseTween = transform.DOScale(baseScale * 1.035f, .85f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData) { pulseTween?.Pause(); ScaleTo(hoverScale); }
    public void OnPointerExit(PointerEventData eventData) { ScaleTo(1f, StartPulse); }
    public void OnPointerDown(PointerEventData eventData) { pulseTween?.Pause(); ScaleTo(pressedScale); }
    public void OnPointerUp(PointerEventData eventData) { ScaleTo(hoverScale); }

    void ScaleTo(float multiplier, TweenCallback complete = null)
    {
        transform.DOKill(false);
        transform.DOScale(baseScale * multiplier, .16f).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(complete);
    }
}
