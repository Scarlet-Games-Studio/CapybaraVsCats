using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    [Min(0.1f)] public float speed = 3f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    [Min(0.1f)] public float fireRate = 1f;
    public int maxHits = 2;
    public SpriteRenderer sr;

    [Header("Movement")]
    [SerializeField, Min(0f)] float horizontalDrift = 0.35f;
    [SerializeField, Min(0.1f)] float driftFrequency = 1.25f;
    [SerializeField, Range(0f, 0.5f)] float despawnMargin = 0.15f;
    [SerializeField, Min(0.1f)] float minimumMoveSpeed = 0.35f;
    [SerializeField, Min(0)] int contactDamage = 10;

    EnemyHealth enemyHealth;
    Camera gameCamera;
    float nextFire;
    float driftSeed;
    float damageFlashEndsAt;
    Color normalColor;
    bool enteredScreen;

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) normalColor = sr.color;
        driftSeed = Random.Range(0f, Mathf.PI * 2f);
    }

    void Start()
    {
        gameCamera = Camera.main;
        nextFire = Time.time + Random.Range(0.15f, Mathf.Max(0.2f, fireRate));
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        Move();
        bool inside = IsWithinScreenBounds();
        if (inside) enteredScreen = true;

        if (inside && projectilePrefab != null && firePoint != null && Time.time >= nextFire)
        {
            GameObject projectile = ProjectileSpawnLimiter.Spawn(projectilePrefab, firePoint.position, firePoint.rotation);
            if (projectile != null) FMODManager.PlayEnemyShot(firePoint.position);
            nextFire = Time.time + Mathf.Max(0.1f, fireRate);
        }

        if (enteredScreen && IsPastDespawnBounds()) Destroy(gameObject);

        if (sr != null && damageFlashEndsAt > 0f && Time.time >= damageFlashEndsAt)
        {
            sr.color = normalColor;
            damageFlashEndsAt = 0f;
        }
    }

    void Move()
    {
        float verticalSpeed = Mathf.Max(minimumMoveSpeed, speed);
        float drift = Mathf.Sin(Time.time * driftFrequency + driftSeed) * horizontalDrift;
        transform.Translate(new Vector2(drift, -verticalSpeed) * Time.deltaTime, Space.World);
    }

    bool IsWithinScreenBounds()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null) return true;
        Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
        return viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
    }

    bool IsPastDespawnBounds()
    {
        if (gameCamera == null) return false;
        Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
        return viewport.z <= 0f || viewport.x < -despawnMargin || viewport.x > 1f + despawnMargin ||
               viewport.y < -despawnMargin;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Health playerHealth = other.GetComponentInParent<Health>();
        if (playerHealth == null || !playerHealth.CompareTag("Player")) return;
        playerHealth.TakeDamage(contactDamage);
        FMODManager.PlayEnemyDeath(transform.position);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Health playerHealth = collision.gameObject.GetComponentInParent<Health>();
        if (playerHealth == null || !playerHealth.CompareTag("Player")) return;
        playerHealth.TakeDamage(contactDamage);
        FMODManager.PlayEnemyDeath(transform.position);
        Destroy(gameObject);
    }

    public void FlashDamage()
    {
        if (sr == null || !isActiveAndEnabled) return;
        sr.color = Color.red;
        damageFlashEndsAt = Time.time + 0.05f;
    }

    void OnDisable()
    {
        if (sr != null) sr.color = normalColor;
        damageFlashEndsAt = 0f;
    }
}
