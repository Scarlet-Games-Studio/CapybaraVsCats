using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerController : MonoBehaviour
{
    [Min(0.1f)] public float moveSpeed = 5f;
    [Min(0.05f)] public float fireRate = 0.5f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform firePoint2;
    [SerializeField] bool autoFire = true;
    [SerializeField] bool secondaryFireUnlocked;

    float nextFire;
    Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Start()
    {
        UpdateHealthUI();
    }

    void Update()
    {
        if (health == null || health.IsDead || Time.timeScale <= 0f) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        if (autoFire || Input.GetButton("Fire1")) Shoot();
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (UIManager.instance != null && health != null)
            UIManager.instance.UpdateHealth(health.currentHealth);
    }

    void Shoot()
    {
        if (Time.time < nextFire || projectilePrefab == null || firePoint == null) return;
        nextFire = Time.time + Mathf.Max(0.05f, fireRate);
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (secondaryFireUnlocked && firePoint2 != null && firePoint2 != firePoint)
            Instantiate(projectilePrefab, firePoint2.position, firePoint2.rotation);
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
        if (secondaryFireUnlocked || firePoint2 == null || firePoint2 == firePoint) return false;
        secondaryFireUnlocked = true;
        return true;
    }
}
