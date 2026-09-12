using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Min(0.1f)] public float moveSpeed = 5f;
    [Min(0.05f)] public float fireRate = 0.5f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform firePoint2;
    [SerializeField] bool secondaryFireUnlocked;
    [SerializeField, Range(1, 3)] int fireTier = 1;

    public int FireTier => Mathf.Clamp(fireTier, 1, 3);

    float nextFire;
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
        if (secondaryFireUnlocked) fireTier = Mathf.Max(fireTier, 2);
        fireTier = Mathf.Clamp(fireTier, 1, 3);
    }

    void Start()
    {
        UpdateHealthUI();
        FMODManager.StartPlayerEngine(gameObject);
    }

    void OnDestroy()
    {
        FMODManager.StopPlayerEngine(gameObject);
    }

    void Update()
    {
        if (health == null || health.IsDead || Time.timeScale <= 0f) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        if (Input.GetButton("Fire1")) Shoot();
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (UIManager.instance != null && health != null)
            UIManager.instance.UpdateHealth(health.currentHealth);
    }

    public void Shoot()
    {
        if (health == null || health.IsDead || Time.timeScale <= 0f) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;
        if (Time.time < nextFire || projectilePrefab == null || firePoint == null) return;
        nextFire = Time.time + Mathf.Max(0.05f, fireRate);
        FireVolley();
        FMODManager.PlayPlayerShot(firePoint.position);
    }

    void FireVolley()
    {
        if (FireTier == 1)
        {
            SpawnProjectile(firePoint.position, firePoint.rotation);
            return;
        }

        bool hasSecondPoint = firePoint2 != null && firePoint2 != firePoint;
        Vector3 centerPosition = hasSecondPoint ? (firePoint.position + firePoint2.position) * 0.5f : firePoint.position;
        Vector3 horizontalAxis = hasSecondPoint ? firePoint2.position - firePoint.position : firePoint.right;
        if (horizontalAxis.sqrMagnitude < 0.0001f) horizontalAxis = firePoint.right;
        horizontalAxis.Normalize();

        // Garante uma separação visual legível mesmo quando os FirePoints do
        // prefab estão muito próximos por causa da escala da nave.
        float halfSeparation = hasSecondPoint
            ? Mathf.Max(Vector3.Distance(firePoint.position, firePoint2.position) * 0.72f, 0.1f)
            : 0.1f;
        Vector3 leftPosition = centerPosition - horizontalAxis * halfSeparation;
        Vector3 rightPosition = centerPosition + horizontalAxis * halfSeparation;

        if (FireTier == 2)
        {
            SpawnProjectile(leftPosition, firePoint.rotation);
            SpawnProjectile(rightPosition, hasSecondPoint ? firePoint2.rotation : firePoint.rotation);
            return;
        }

        SpawnProjectile(leftPosition, firePoint.rotation * Quaternion.Euler(0f, 0f, 7f));
        SpawnProjectile(centerPosition, firePoint.rotation);
        SpawnProjectile(rightPosition, (hasSecondPoint ? firePoint2.rotation : firePoint.rotation) * Quaternion.Euler(0f, 0f, -7f));
    }

    void SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        ProjectileSpawnLimiter.Spawn(projectilePrefab, position, rotation);
    }

    public void IncreaseFireRate(float amount)
    {
        if (amount <= 0f) return;
        fireRate = Mathf.Max(0.08f, fireRate - amount);
    }

    public void IncreaseFireRatePercent(float percentage)
    {
        if (percentage <= 0f) return;
        fireRate = Mathf.Max(0.08f, fireRate * (1f - Mathf.Clamp(percentage, 0f, 0.8f)));
    }

    public void IncreaseSpeed(float amount)
    {
        if (amount > 0f) moveSpeed = Mathf.Min(15f, moveSpeed + amount);
    }

    public void IncreaseSpeedPercent(float percentage)
    {
        if (percentage > 0f) moveSpeed = Mathf.Min(15f, moveSpeed * (1f + percentage));
    }

    public bool PowerUpBullet()
    {
        return UpgradeFireMode(2);
    }

    public bool UpgradeFireMode(int targetTier)
    {
        targetTier = Mathf.Clamp(targetTier, 1, 3);
        if (targetTier <= FireTier) return false;
        fireTier = targetTier;
        secondaryFireUnlocked = fireTier >= 2;
        return true;
    }
}
