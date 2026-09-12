using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Min(0.1f)] public float speed = 10f;
    [Min(0.1f)] public float lifeTime = 2f;
    [Min(1)] public int damage = 10;
    [Tooltip("Maior dimensão visual do tiro em unidades do mundo. Zero preserva o tamanho original do prefab.")]
    [Min(0f)] public float visualSize;

    Camera gameCamera;
    Rigidbody2D body;
    bool enteredScreen;
    bool consumed;
    bool spawnLimiterRegistered;

    public void RegisterSpawnLimiter() => spawnLimiterRegistered = true;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        ProjectileVisuals.EnsureVisible(gameObject, visualSize);
    }

    void Start()
    {
        gameCamera = Camera.main;
        // A margem evita que tiros lentos desapareçam no meio da tela.
        Destroy(gameObject, Mathf.Max(lifeTime, 8f));
    }

    void Update()
    {
        // Tiros comuns descem; os tiros do boss preservam a direção aplicada pelo padrão radial.
        if (body == null || body.linearVelocity.sqrMagnitude < 0.0001f)
            transform.Translate(Vector2.down * speed * Time.deltaTime, Space.World);

        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null) return;

        Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
        bool inside = viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
        enteredScreen |= inside;

        // Permite que o projétil nasça pouco fora da tela e entre antes de ser descartado.
        if (enteredScreen && IsFarOutsideScreen(viewport)) Destroy(gameObject);
    }

    static bool IsFarOutsideScreen(Vector3 viewport)
    {
        return viewport.z <= 0f || viewport.x < -0.15f || viewport.x > 1.15f ||
               viewport.y < -0.15f || viewport.y > 1.15f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (consumed) return;

        Health playerHealth = collision.GetComponentInParent<Health>();
        if (playerHealth == null || !playerHealth.CompareTag("Player")) return;

        consumed = true;
        playerHealth.TakeDamage(damage);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (spawnLimiterRegistered) ProjectileSpawnLimiter.Release(false);
    }
}
