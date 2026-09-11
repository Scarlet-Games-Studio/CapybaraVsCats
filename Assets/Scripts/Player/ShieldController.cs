using System;
using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField, Min(1)] int maximumHits = 3;
    [SerializeField, Min(0f)] float hitInvulnerability = 0.25f;

    public int RemainingHits { get; private set; }
    public bool IsActive => RemainingHits > 0;
    public event Action<int, int> Changed;

    GameObject visual;
    SpriteRenderer[] layers;
    Coroutine cooldownRoutine;
    bool invulnerable;

    public void Activate(GameObject shieldPrefab, int hits)
    {
        if (hits <= 0) return;
        maximumHits = Mathf.Max(maximumHits, hits);
        RemainingHits = Mathf.Min(maximumHits, RemainingHits + hits);

        if (visual == null && shieldPrefab != null)
            CreateVisual(shieldPrefab);

        RefreshVisual();
        Changed?.Invoke(RemainingHits, maximumHits);
    }

    void CreateVisual(GameObject shieldPrefab)
    {
        visual = Instantiate(shieldPrefab, transform);
        visual.name = $"Active Shield - {shieldPrefab.name}";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        layers = visual.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer layer in layers)
        {
            // Os prefabs antigos foram salvos em coordenadas de cena (-99, etc.).
            // Centralizar cada camada torna o escudo correto em qualquer nave.
            layer.transform.localPosition = Vector3.zero;
            layer.transform.localRotation = Quaternion.identity;
            layer.sortingLayerName = "Characters";
            layer.sortingOrder = 10;
        }

        foreach (Collider2D visualCollider in visual.GetComponentsInChildren<Collider2D>(true))
            visualCollider.enabled = false;
    }

    public bool TryAbsorbHit()
    {
        if (RemainingHits <= 0) return false;
        if (invulnerable) return true;

        RemainingHits--;
        RefreshVisual();
        Changed?.Invoke(RemainingHits, maximumHits);

        if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        cooldownRoutine = StartCoroutine(HitCooldown());
        if (RemainingHits == 0) RemoveVisual();
        return true;
    }

    void RefreshVisual()
    {
        if (layers == null || layers.Length == 0) return;
        int visibleLayers = Mathf.CeilToInt((float)RemainingHits / maximumHits * layers.Length);
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].enabled = i < visibleLayers;
            Color color = layers[i].color;
            color.a = Mathf.Lerp(0.45f, 1f, (float)RemainingHits / maximumHits);
            layers[i].color = color;
        }
    }

    void RemoveVisual()
    {
        if (visual != null) Destroy(visual, 0.12f);
        visual = null;
        layers = null;
    }

    IEnumerator HitCooldown()
    {
        invulnerable = true;
        if (layers != null)
            foreach (SpriteRenderer layer in layers) if (layer != null) layer.color = new Color(1f, 0.45f, 0.45f, layer.color.a);

        yield return new WaitForSeconds(hitInvulnerability);
        invulnerable = false;
        cooldownRoutine = null;
        RefreshVisual();
    }

    void OnDisable()
    {
        invulnerable = false;
        cooldownRoutine = null;
    }
}
