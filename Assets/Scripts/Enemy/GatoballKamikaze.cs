using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
public class GatoballKamikaze : MonoBehaviour
{
    [SerializeField] float chaseSpeed = 1.65f;
    [SerializeField] float turnSpeed = 2.8f;
    [SerializeField] int collisionDamage = 25;
    [SerializeField, Min(0.1f)] float visualScale = 0.25f;
    [Header("Formation entry")]
    [SerializeField, Min(0f)] float formationDuration = 2.5f;
    [SerializeField, Min(0.1f)] float formationSpeed = 1.15f;
    [SerializeField, Min(1f)] float maximumLifetime = 12f;
    [SerializeField, Range(0f, 0.25f)] float despawnMargin = 0.08f;

    Transform target;
    Rigidbody2D body;
    float chaseStartTime;
    float destroyTime;
    bool enteredScreen;

    void Awake()
    {
        transform.localScale = Vector3.one * visualScale;
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
        chaseStartTime = Time.time + formationDuration;
        destroyTime = Time.time + maximumLifetime;

        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = true;
            renderer.sortingLayerName = "Characters";
            renderer.sortingOrder = 5;
        }
    }

    void FixedUpdate()
    {
        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
            return;
        }

        if (ShouldDespawnAfterLeavingScreen())
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time < chaseStartTime)
        {
            body.linearVelocity = Vector2.down * formationSpeed;
            transform.up = Vector2.up;
            return;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        Vector2 direction = target != null
            ? ((Vector2)target.position - body.position).normalized
            : Vector2.down;
        Vector2 desiredVelocity = direction * chaseSpeed;
        body.linearVelocity = Vector2.Lerp(body.linearVelocity, desiredVelocity, turnSpeed * Time.fixedDeltaTime);

        if (direction.sqrMagnitude > 0.01f)
            transform.up = -direction;
    }

    bool ShouldDespawnAfterLeavingScreen()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null) return false;

        Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
        bool inside = viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f &&
                      viewport.y >= 0f && viewport.y <= 1f;
        if (inside)
        {
            enteredScreen = true;
            return false;
        }

        if (!enteredScreen) return false;
        return viewport.z <= 0f || viewport.x < -despawnMargin || viewport.x > 1f + despawnMargin ||
               viewport.y < -despawnMargin || viewport.y > 1f + despawnMargin;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Health playerHealth = other.GetComponentInParent<Health>();
        if (playerHealth == null || !playerHealth.CompareTag("Player")) return;
        playerHealth.TakeDamage(collisionDamage);
        Destroy(gameObject); // impacto kamikaze não conta como morte e não gera drop
    }
}
