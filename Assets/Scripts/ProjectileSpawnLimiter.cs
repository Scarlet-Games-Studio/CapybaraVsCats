using UnityEngine;

/// <summary>
/// Impede que uma sequência de spawn defeituosa acumule milhares de tiros e
/// congele o jogo. Os limites preservam uma quantidade alta de bullet hell.
/// </summary>
public static class ProjectileSpawnLimiter
{
    const int MaximumPlayerProjectiles = 100;
    const int MaximumEnemyProjectiles = 240;

    static int activePlayerProjectiles;
    static int activeEnemyProjectiles;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetCounters()
    {
        activePlayerProjectiles = 0;
        activeEnemyProjectiles = 0;
    }

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        bool playerProjectile = prefab.CompareTag("PlayerProjectile");
        int current = playerProjectile ? activePlayerProjectiles : activeEnemyProjectiles;
        int maximum = playerProjectile ? MaximumPlayerProjectiles : MaximumEnemyProjectiles;
        if (current >= maximum) return null;

        if (playerProjectile) activePlayerProjectiles++;
        else activeEnemyProjectiles++;

        GameObject projectile = Object.Instantiate(prefab, position, rotation);
        if (playerProjectile)
        {
            Projectile behaviour = projectile.GetComponent<Projectile>();
            if (behaviour != null) behaviour.RegisterSpawnLimiter();
            else Release(true);
        }
        else
        {
            EnemyProjectile behaviour = projectile.GetComponent<EnemyProjectile>();
            if (behaviour != null) behaviour.RegisterSpawnLimiter();
            else Release(false);
        }

        return projectile;
    }

    public static void Release(bool playerProjectile)
    {
        if (playerProjectile) activePlayerProjectiles = Mathf.Max(0, activePlayerProjectiles - 1);
        else activeEnemyProjectiles = Mathf.Max(0, activeEnemyProjectiles - 1);
    }
}
