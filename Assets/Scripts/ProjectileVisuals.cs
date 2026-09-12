using UnityEngine;

/// <summary>
/// Normaliza somente a apresentação dos tiros. A área de colisão original é preservada.
/// </summary>
public static class ProjectileVisuals
{
    const int ProjectileSortingOrder = 30;

    public static void EnsureVisible(GameObject projectile)
    {
        EnsureVisible(projectile, 0f);
    }

    public static void EnsureVisible(GameObject projectile, float targetVisibleSize)
    {
        if (projectile == null) return;

        SpriteRenderer[] renderers = projectile.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0) return;

        float largestDimension = 0f;
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.forceRenderingOff = false;
            spriteRenderer.sortingLayerName = "Characters";
            spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, ProjectileSortingOrder);

            Color color = spriteRenderer.color;
            if (color.a < 0.95f)
            {
                color.a = 1f;
                spriteRenderer.color = color;
            }

            largestDimension = Mathf.Max(largestDimension,
                spriteRenderer.bounds.size.x,
                spriteRenderer.bounds.size.y);
        }

        if (largestDimension <= 0f || targetVisibleSize <= 0f) return;

        float multiplier = Mathf.Clamp(targetVisibleSize / largestDimension, 0.1f, 10f);
        projectile.transform.localScale *= multiplier;
        PreserveColliderWorldSize(projectile, multiplier);
    }

    static void PreserveColliderWorldSize(GameObject projectile, float multiplier)
    {
        foreach (BoxCollider2D box in projectile.GetComponentsInChildren<BoxCollider2D>(true))
        {
            box.size /= multiplier;
            box.offset /= multiplier;
        }

        foreach (CircleCollider2D circle in projectile.GetComponentsInChildren<CircleCollider2D>(true))
        {
            circle.radius /= multiplier;
            circle.offset /= multiplier;
        }

        foreach (CapsuleCollider2D capsule in projectile.GetComponentsInChildren<CapsuleCollider2D>(true))
        {
            capsule.size /= multiplier;
            capsule.offset /= multiplier;
        }
    }
}
