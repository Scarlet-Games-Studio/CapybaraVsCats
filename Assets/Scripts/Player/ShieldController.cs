using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField, Range(1, 4)] int maximumHits = 4;
    [SerializeField, Min(0f)] float hitInvulnerability = 0.25f;
    [SerializeField, Min(1f)] float fullDuration = 16f;

    public int RemainingHits { get; private set; }
    public bool IsActive => RemainingHits > 0;
    public float RemainingDuration => IsActive ? Mathf.Max(0f, nextDecayAt - Time.time + phaseDuration * (RemainingHits - 1)) : 0f;
    public event Action<int, int> Changed;

    GameObject visual;
    SpriteRenderer[] layers;
    Coroutine cooldownRoutine;
    Tween dangerTween;
    Vector3 visualScale;
    float phaseDuration;
    float nextDecayAt;
    bool invulnerable;

    public void Activate(GameObject shieldPrefab, int hits)
    {
        Activate(shieldPrefab, hits, fullDuration);
    }

    public void Activate(GameObject shieldPrefab, int hits, float duration)
    {
        if (hits <= 0) return;
        maximumHits = Mathf.Clamp(hits, 1, 4);
        fullDuration = Mathf.Max(1f, duration);
        phaseDuration = fullDuration / maximumHits;
        RemainingHits = maximumHits;
        nextDecayAt = Time.time + phaseDuration;

        if (visual == null && shieldPrefab != null)
            CreateVisual(shieldPrefab);
        else if (visual != null)
        {
            visual.transform.DOKill(false);
            visual.transform.localScale = visualScale;
        }

        RefreshVisual();
        Changed?.Invoke(RemainingHits, maximumHits);
    }

    void Update()
    {
        if (!IsActive || Time.timeScale <= 0f) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;
        if (Time.time < nextDecayAt) return;

        ConsumePhase(false);
        nextDecayAt = Time.time + phaseDuration;
    }

    void CreateVisual(GameObject shieldPrefab)
    {
        SpriteRenderer playerRenderer = GetComponentInChildren<SpriteRenderer>();
        float targetWorldSize = playerRenderer != null
            ? Mathf.Max(playerRenderer.bounds.size.x, playerRenderer.bounds.size.y) * 1.55f
            : 1.5f;

        visual = Instantiate(shieldPrefab, transform);
        visual.name = $"Active Shield - {shieldPrefab.name}";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        layers = visual.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer layer in layers)
        {
            layer.transform.localPosition = Vector3.zero;
            layer.transform.localRotation = Quaternion.identity;
            layer.enabled = true;
            layer.sortingLayerName = "Characters";
            layer.sortingOrder = 10;
        }

        foreach (Collider2D visualCollider in visual.GetComponentsInChildren<Collider2D>(true))
            visualCollider.enabled = false;

        Bounds combined = default;
        bool hasBounds = false;
        foreach (SpriteRenderer layer in layers)
        {
            if (layer.sprite == null) continue;
            if (!hasBounds) { combined = layer.bounds; hasBounds = true; }
            else combined.Encapsulate(layer.bounds);
        }

        if (hasBounds)
        {
            float currentSize = Mathf.Max(combined.size.x, combined.size.y);
            if (currentSize > 0.001f)
                visual.transform.localScale *= targetWorldSize / currentSize;
        }

        visualScale = visual.transform.localScale;
    }

    public bool TryAbsorbHit()
    {
        if (invulnerable) return true;
        if (!IsActive) return false;

        ConsumePhase(true);
        nextDecayAt = Time.time + phaseDuration;

        if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        cooldownRoutine = StartCoroutine(HitCooldown());
        return true;
    }

    void ConsumePhase(bool animateHit)
    {
        if (!IsActive) return;
        RemainingHits--;

        if (animateHit && visual != null)
        {
            visual.transform.DOKill(false);
            visual.transform.localScale = visualScale;
            visual.transform.DOPunchScale(visualScale * 0.13f, 0.2f, 6, 0.45f);
        }

        RefreshVisual();
        Changed?.Invoke(RemainingHits, maximumHits);
        if (RemainingHits == 0) RemoveVisual();
    }

    void RefreshVisual()
    {
        dangerTween?.Kill();
        dangerTween = null;
        if (layers == null || layers.Length == 0) return;

        float damage = maximumHits <= 1 ? 1f : 1f - (RemainingHits - 1f) / (maximumHits - 1f);
        int activeLayer = Mathf.Clamp(Mathf.RoundToInt(damage * (layers.Length - 1)), 0, layers.Length - 1);

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].enabled = RemainingHits > 0 && i == activeLayer;
            Color color = layers[i].color;
            color.r = color.g = color.b = 1f;
            color.a = RemainingHits == 1 ? 0.62f : 1f;
            layers[i].color = color;
        }

        if (RemainingHits == 1 && visual != null)
        {
            visual.transform.localScale = visualScale;
            dangerTween = visual.transform.DOScale(visualScale * 1.08f, 0.32f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(visual);
        }
    }

    void RemoveVisual()
    {
        dangerTween?.Kill();
        dangerTween = null;
        GameObject visualToRemove = visual;
        if (visualToRemove != null)
        {
            visualToRemove.transform.DOKill();
            visualToRemove.transform.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (visualToRemove != null) Destroy(visualToRemove);
            });
        }
        visual = null;
        layers = null;
    }

    IEnumerator HitCooldown()
    {
        invulnerable = true;
        yield return new WaitForSeconds(hitInvulnerability);
        invulnerable = false;
        cooldownRoutine = null;
    }

    void OnDisable()
    {
        invulnerable = false;
        cooldownRoutine = null;
        dangerTween?.Kill();
    }

    void OnDestroy()
    {
        dangerTween?.Kill();
        if (visual != null) visual.transform.DOKill();
    }
}
