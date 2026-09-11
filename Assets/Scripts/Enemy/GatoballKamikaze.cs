using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
public class GatoballKamikaze : MonoBehaviour
{
    enum KamikazeState { Formation, Telegraph, Chase }

    [Header("Chase")]
    [SerializeField, Min(0.1f)] float chaseSpeed = 2.8f;
    [SerializeField, Min(0.1f)] float maximumChaseSpeed = 5.2f;
    [SerializeField, Min(0f)] float acceleration = 0.65f;
    [SerializeField, Min(0.1f)] float turnSpeed = 4.5f;
    [SerializeField, Range(0f, 0.75f)] float targetPrediction = 0.28f;
    [SerializeField, Min(1)] int collisionDamage = 25;

    [Header("Formation entry")]
    [SerializeField, Min(0f)] float formationDuration = 1.35f;
    [SerializeField, Min(0.1f)] float formationSpeed = 1.5f;
    [SerializeField, Min(0.05f)] float telegraphDuration = 0.55f;
    [SerializeField, Min(1f)] float maximumLifetime = 14f;
    [SerializeField, Range(0f, 0.5f)] float despawnMargin = 0.18f;

    [Header("Visual feedback")]
    [SerializeField, Min(0.05f)] float visualScale = 0.72f;
    [SerializeField, Min(0f)] float pulseAmount = 0.1f;
    [SerializeField, Min(0f)] float pulseSpeed = 10f;
    [SerializeField] Color telegraphColor = new Color(1f, 0.32f, 0.12f, 1f);
    [SerializeField] Color chaseColor = Color.white;
    [SerializeField] bool disableLegacyAnimator = true;

    Transform target;
    Rigidbody2D body;
    Rigidbody2D targetBody;
    SpriteRenderer spriteRenderer;
    TrailRenderer trail;
    Material trailMaterial;
    KamikazeState state;
    Vector3 baseScale;
    float stateEndsAt;
    float destroyTime;
    float currentSpeed;
    bool enteredScreen;
    bool impacted;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        Collider2D hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
        if (hitbox is BoxCollider2D box) box.size = new Vector2(0.82f, 0.82f);

        Animator legacyAnimator = GetComponent<Animator>();
        if (legacyAnimator != null && disableLegacyAnimator) legacyAnimator.enabled = false;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = renderer == spriteRenderer;
            renderer.sortingLayerName = "Characters";
            renderer.sortingOrder = 7;
        }

        baseScale = Vector3.one * visualScale;
        transform.localScale = baseScale;
        currentSpeed = chaseSpeed;
        destroyTime = Time.time + maximumLifetime;
        EnterState(KamikazeState.Formation, formationDuration);
        CreateTrail();
    }

    void FixedUpdate()
    {
        if (Time.time >= destroyTime || ShouldDespawnAfterLeavingScreen())
        {
            Destroy(gameObject);
            return;
        }

        if (GameManager.instance != null && !GameManager.instance.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        UpdateState();
        switch (state)
        {
            case KamikazeState.Formation:
                body.linearVelocity = Vector2.down * formationSpeed;
                break;
            case KamikazeState.Telegraph:
                body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, 0.18f);
                break;
            case KamikazeState.Chase:
                ChaseTarget();
                break;
        }

        if (body.linearVelocity.sqrMagnitude > 0.01f)
            transform.up = -body.linearVelocity.normalized;
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        if (state == KamikazeState.Telegraph)
        {
            float flash = Mathf.PingPong(Time.unscaledTime * 8f, 1f);
            spriteRenderer.color = Color.Lerp(Color.white, telegraphColor, flash);
        }
        else
        {
            spriteRenderer.color = chaseColor;
        }

        float pulse = state == KamikazeState.Chase
            ? 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount
            : 1f;
        transform.localScale = baseScale * pulse;
    }

    void UpdateState()
    {
        if (Time.time < stateEndsAt) return;
        if (state == KamikazeState.Formation)
            EnterState(KamikazeState.Telegraph, telegraphDuration);
        else if (state == KamikazeState.Telegraph)
            EnterState(KamikazeState.Chase, 0f);
    }

    void EnterState(KamikazeState newState, float duration)
    {
        state = newState;
        stateEndsAt = Time.time + duration;
        if (newState == KamikazeState.Chase) AcquireTarget();
    }

    void ChaseTarget()
    {
        if (target == null) AcquireTarget();

        Vector2 aimPoint = target != null ? (Vector2)target.position : body.position + Vector2.down;
        if (targetBody != null) aimPoint += targetBody.linearVelocity * targetPrediction;

        Vector2 direction = (aimPoint - body.position).normalized;
        currentSpeed = Mathf.MoveTowards(currentSpeed, maximumChaseSpeed, acceleration * Time.fixedDeltaTime);
        Vector2 desiredVelocity = direction * currentSpeed;
        float steering = 1f - Mathf.Exp(-turnSpeed * Time.fixedDeltaTime);
        body.linearVelocity = Vector2.Lerp(body.linearVelocity, desiredVelocity, steering);
    }

    void AcquireTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        target = player != null ? player.transform : null;
        targetBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
    }

    void CreateTrail()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) return;

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.32f;
        trail.minVertexDistance = 0.04f;
        trail.startWidth = 0.16f;
        trail.endWidth = 0f;
        trail.sortingLayerName = "Characters";
        trail.sortingOrder = 6;
        trailMaterial = new Material(shader) { name = "Gatoball Trail (Runtime)" };
        trail.material = trailMaterial;
        trail.startColor = new Color(1f, 0.28f, 0.05f, 0.75f);
        trail.endColor = new Color(1f, 0.05f, 0.02f, 0f);
        trail.emitting = false;
    }

    void LateUpdate()
    {
        if (trail != null) trail.emitting = state == KamikazeState.Chase;
    }

    bool ShouldDespawnAfterLeavingScreen()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null) return false;

        Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
        bool inside = viewport.z > 0f && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
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
        if (impacted) return;
        Health playerHealth = other.GetComponentInParent<Health>();
        if (playerHealth == null || !playerHealth.CompareTag("Player")) return;

        impacted = true;
        playerHealth.TakeDamage(collisionDamage);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (trailMaterial != null) Destroy(trailMaterial);
    }
}
