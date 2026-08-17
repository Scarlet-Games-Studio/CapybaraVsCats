using System.Collections;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public int RemainingHits { get; private set; }
    GameObject visual;
    SpriteRenderer[] layers;
    bool invulnerable;

    public void Activate(GameObject shieldPrefab, int hits)
    {
        RemainingHits = Mathf.Max(RemainingHits, hits);
        if (visual != null) Destroy(visual);
        if (shieldPrefab != null)
        {
            visual = Instantiate(shieldPrefab, transform);
            visual.name = $"Active Shield - {shieldPrefab.name}";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            layers = visual.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer layer in layers)
            {
                layer.sortingLayerName = "Characters";
                layer.sortingOrder = 10;
            }
            RefreshVisual();
        }
    }

    public bool TryAbsorbHit()
    {
        if (RemainingHits <= 0 || invulnerable) return invulnerable;
        RemainingHits--;
        RefreshVisual();
        StartCoroutine(HitCooldown());
        if (RemainingHits == 0 && visual != null) Destroy(visual, 0.15f);
        return true;
    }

    void RefreshVisual()
    {
        if (layers == null) return;
        for (int i = 0; i < layers.Length; i++)
            layers[i].enabled = i < RemainingHits;
    }

    IEnumerator HitCooldown()
    {
        invulnerable = true;
        yield return new WaitForSeconds(0.25f);
        invulnerable = false;
    }
}
