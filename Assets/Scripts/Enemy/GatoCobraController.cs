using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth), typeof(Rigidbody2D))]
public sealed class GatoCobraController : MonoBehaviour
{
    public enum MovementPattern
    {
        Random,
        WideSerpentine,
        TightSnake,
        SCurve,
        PartialLoop,
        ScreenSweep,
        Dive,
        Coil
    }

    struct PathSample
    {
        public float distance;
        public Vector3 position;

        public PathSample(float sampleDistance, Vector3 samplePosition)
        {
            distance = sampleDistance;
            position = samplePosition;
        }
    }

    [Header("Formation")]
    [SerializeField] Transform[] bodySegments;
    [Tooltip("Distancia percorrida entre o centro de cada segmento do corpo.")]
    [SerializeField, Min(0.05f)] float segmentSpacing = 0.3f;
    [SerializeField, Min(1f)] float segmentRotationSpeed = 14f;

    [Header("Head Route")]
    [SerializeField] MovementPattern movementPattern = MovementPattern.Random;
    [SerializeField, Min(0.05f)] float speed = 0.32f;
    [SerializeField, Range(0.05f, 0.45f)] float viewportWaveAmplitude = 0.3f;
    [SerializeField, Range(0.5f, 5f)] float wavePeriod = 2f;
    [SerializeField, Range(0f, 0.35f)] float horizontalViewportPadding = 0.1f;
    [SerializeField, Range(0.02f, 0.5f)] float steeringSmoothTime = 0.12f;

    [Header("Attack")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;
    [SerializeField, Min(0.1f)] float fireRate = 1.6f;
    [Tooltip("Intervalo entre rajadas laterais de um segmento do corpo.")]
    [SerializeField, Min(0.5f)] float sideShotInterval = 3f;
    [SerializeField, Min(1)] int contactDamage = 25;
    [SerializeField, Min(0.1f)] float contactDamageCooldown = 0.75f;

    readonly List<PathSample> headHistory = new List<PathSample>(512);

    Camera gameCamera;
    Transform player;
    SpriteRenderer[] renderers;
    Color[] normalColors;
    MovementPattern activePattern;
    float spawnTime;
    float initialViewportX;
    float baseViewportY;
    float patternSeed;
    float horizontalVelocity;
    float pathDistance;
    Vector3 lastRecordedPosition;
    float nextFireTime;
    float nextSideShotTime;
    float nextContactDamageTime;
    float damageFlashEndsAt;
    int headShots;
    bool enteredScreen;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        normalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) normalColors[i] = renderers[i].color;

        if (bodySegments == null) bodySegments = System.Array.Empty<Transform>();

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    void Start()
    {
        gameCamera = Camera.main;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;

        activePattern = movementPattern == MovementPattern.Random
            ? (MovementPattern)Random.Range(1, System.Enum.GetValues(typeof(MovementPattern)).Length)
            : movementPattern;
        spawnTime = Time.time;
        patternSeed = Random.Range(0f, Mathf.PI * 2f);

        if (gameCamera != null)
        {
            Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
            initialViewportX = Mathf.Clamp(viewport.x, horizontalViewportPadding, 1f - horizontalViewportPadding);
            baseViewportY = viewport.y;
        }
        else
        {
            initialViewportX = 0.5f;
            baseViewportY = 1.1f;
        }

        SeedHistory();
        nextFireTime = Time.time + Random.Range(0.7f, Mathf.Max(0.8f, fireRate));
        nextSideShotTime = Time.time + sideShotInterval;
    }

    void Update()
    {
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        MoveHead();
        RecordHeadPath();
        FollowHeadPath();

        bool visible = IsVisible();
        enteredScreen |= visible;
        if (visible) TryShoot();

        if (damageFlashEndsAt > 0f && Time.time >= damageFlashEndsAt) RestoreColors();

        // A cobra so desaparece quando a cauda inteira saiu do campo de jogo.
        if (enteredScreen && IsEntireBodyBelowScreen()) Destroy(gameObject);
    }

    void SeedHistory()
    {
        headHistory.Clear();
        float requiredDistance = Mathf.Max(0.2f, (bodySegments.Length + 1) * segmentSpacing);
        int samples = Mathf.Max(2, Mathf.CeilToInt(requiredDistance / 0.025f));
        float distanceStep = requiredDistance / samples;
        Vector3 headPosition = transform.position;

        for (int i = samples; i >= 0; i--)
        {
            float distanceBehindHead = i * distanceStep;
            // A formação começa encaixada atrás da cabeça, antes de avançar pela rota.
            Vector3 position = headPosition + Vector3.up * distanceBehindHead;
            headHistory.Add(new PathSample(-distanceBehindHead, position));
        }

        pathDistance = 0f;
        lastRecordedPosition = transform.position;
        FollowHeadPath(true);
    }

    void MoveHead()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null)
        {
            transform.position += Vector3.down * (speed * Time.deltaTime);
            return;
        }

        float elapsed = Time.time - spawnTime;
        Vector3 currentViewport = gameCamera.WorldToViewportPoint(transform.position);
        float desiredX = Mathf.Clamp(DesiredViewportX(elapsed), horizontalViewportPadding, 1f - horizontalViewportPadding);
        float nextX = Mathf.SmoothDamp(currentViewport.x, desiredX, ref horizontalVelocity,
            steeringSmoothTime, Mathf.Infinity, Time.deltaTime);
        float nextY = currentViewport.y - WorldSpeedToViewportSpeed(speed) * VerticalSpeedMultiplier(elapsed) * Time.deltaTime;
        nextY += VerticalPatternOffset(elapsed) - VerticalPatternOffset(Mathf.Max(0f, elapsed - Time.deltaTime));

        Vector3 oldPosition = transform.position;
        Vector3 nextPosition = gameCamera.ViewportToWorldPoint(new Vector3(nextX, nextY, currentViewport.z));
        nextPosition.z = oldPosition.z;
        transform.position = nextPosition;

        Vector3 direction = nextPosition - oldPosition;
        if (direction.sqrMagnitude > 0.000001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, angle),
                1f - Mathf.Exp(-segmentRotationSpeed * Time.deltaTime));
        }
    }

    float DesiredViewportX(float elapsed)
    {
        float safePeriod = Mathf.Max(0.5f, wavePeriod);
        float phase = elapsed * Mathf.PI * 2f / safePeriod + patternSeed;
        float amplitude = viewportWaveAmplitude;

        switch (activePattern)
        {
            case MovementPattern.TightSnake:
                return initialViewportX + Mathf.Sin(phase / 0.62f) * amplitude * 0.62f;

            case MovementPattern.SCurve:
                return 0.5f + Mathf.Sin(elapsed * Mathf.PI / (safePeriod * 1.35f) + patternSeed) * amplitude * 1.08f;

            case MovementPattern.PartialLoop:
                return IsSpecialManeuver(elapsed)
                    ? 0.5f + Mathf.Cos(SpecialPhase(elapsed)) * amplitude * 0.75f
                    : initialViewportX + Mathf.Sin(phase) * amplitude;

            case MovementPattern.ScreenSweep:
                return Mathf.Lerp(horizontalViewportPadding, 1f - horizontalViewportPadding,
                    Mathf.PingPong(elapsed / safePeriod, 1f));

            case MovementPattern.Dive:
                if (IsDiveActive(elapsed) && player != null)
                    return Mathf.Lerp(initialViewportX + Mathf.Sin(phase) * amplitude,
                        gameCamera.WorldToViewportPoint(player.position).x, 0.86f);
                return initialViewportX + Mathf.Sin(phase) * amplitude;

            case MovementPattern.Coil:
                return IsSpecialManeuver(elapsed)
                    ? 0.5f + Mathf.Cos(SpecialPhase(elapsed)) * amplitude * 0.78f
                    : initialViewportX + Mathf.Sin(phase) * amplitude;

            default:
                return initialViewportX + Mathf.Sin(phase) * amplitude;
        }
    }

    float VerticalSpeedMultiplier(float elapsed)
    {
        if (activePattern == MovementPattern.Coil && IsSpecialManeuver(elapsed)) return 0.25f;
        if (activePattern == MovementPattern.PartialLoop && IsSpecialManeuver(elapsed)) return 0.55f;
        if (activePattern == MovementPattern.Dive && IsDiveActive(elapsed)) return 1.65f;
        return 1f;
    }

    float VerticalPatternOffset(float elapsed)
    {
        if (!IsSpecialManeuver(elapsed)) return 0f;
        if (activePattern == MovementPattern.Coil) return Mathf.Sin(SpecialPhase(elapsed)) * 0.3f;
        if (activePattern == MovementPattern.PartialLoop) return Mathf.Sin(SpecialPhase(elapsed)) * 0.2f;
        return 0f;
    }

    static bool IsSpecialManeuver(float elapsed)
    {
        float cycle = Mathf.Repeat(elapsed, 7f);
        return cycle >= 1.5f && cycle <= 5.2f;
    }

    static float SpecialPhase(float elapsed)
    {
        return Mathf.InverseLerp(1.5f, 5.2f, Mathf.Repeat(elapsed, 7f)) * Mathf.PI * 2f;
    }

    static bool IsDiveActive(float elapsed)
    {
        float cycle = Mathf.Repeat(elapsed, 6f);
        return cycle >= 3.7f && cycle <= 5.25f;
    }

    float WorldSpeedToViewportSpeed(float worldSpeed)
    {
        float depth = Mathf.Abs(transform.position.z - gameCamera.transform.position.z);
        float bottom = gameCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth)).y;
        float top = gameCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth)).y;
        return worldSpeed / Mathf.Max(0.01f, Mathf.Abs(top - bottom));
    }

    void RecordHeadPath()
    {
        Vector3 currentPosition = transform.position;
        float movedDistance = Vector3.Distance(lastRecordedPosition, currentPosition);
        if (movedDistance > 0.00001f)
        {
            pathDistance += movedDistance;
            headHistory.Add(new PathSample(pathDistance, currentPosition));
        }
        lastRecordedPosition = currentPosition;

        float oldestNeeded = pathDistance - (bodySegments.Length + 1) * segmentSpacing - 0.5f;
        int removeCount = 0;
        while (removeCount < headHistory.Count - 2 && headHistory[removeCount + 1].distance < oldestNeeded)
            removeCount++;
        if (removeCount > 0) headHistory.RemoveRange(0, removeCount);
    }

    void FollowHeadPath(bool immediate = false)
    {
        for (int i = 0; i < bodySegments.Length; i++)
        {
            Transform segment = bodySegments[i];
            if (segment == null) continue;

            float targetDistance = pathDistance - (i + 1) * segmentSpacing;
            Vector3 targetPosition = SamplePath(targetDistance, out Vector3 direction);
            segment.position = targetPosition;

            if (direction.sqrMagnitude <= 0.000001f) continue;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            segment.rotation = immediate
                ? targetRotation
                : Quaternion.Slerp(segment.rotation, targetRotation,
                    1f - Mathf.Exp(-segmentRotationSpeed * Time.deltaTime));
        }
    }

    Vector3 SamplePath(float targetDistance, out Vector3 direction)
    {
        if (headHistory.Count == 0)
        {
            direction = Vector3.down;
            return transform.position;
        }

        if (targetDistance <= headHistory[0].distance)
        {
            direction = Vector3.down;
            return headHistory[0].position;
        }

        for (int i = headHistory.Count - 1; i > 0; i--)
        {
            PathSample newer = headHistory[i];
            PathSample older = headHistory[i - 1];
            if (targetDistance < older.distance) continue;

            float t = Mathf.InverseLerp(older.distance, newer.distance, targetDistance);
            direction = newer.position - older.position;
            return Vector3.LerpUnclamped(older.position, newer.position, t);
        }

        direction = Vector3.down;
        return headHistory[headHistory.Count - 1].position;
    }

    void TryShoot()
    {
        if (projectilePrefab == null || firePoint == null || Time.time < nextFireTime) return;

        SpawnDirectedProjectile(firePoint.position, firePoint.rotation, -firePoint.up);
        nextFireTime = Time.time + fireRate;
        headShots++;

        if (Time.time >= nextSideShotTime && bodySegments.Length > 0 && headShots % 3 == 0)
        {
            Transform source = bodySegments[bodySegments.Length / 2];
            if (source != null)
            {
                SpawnDirectedProjectile(source.position, source.rotation, source.right);
                SpawnDirectedProjectile(source.position, source.rotation, -source.right);
            }
            nextSideShotTime = Time.time + sideShotInterval;
        }
    }

    void SpawnDirectedProjectile(Vector3 position, Quaternion rotation, Vector2 direction)
    {
        GameObject projectile = ProjectileSpawnLimiter.Spawn(projectilePrefab, position, rotation);
        if (projectile == null) return;

        EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null) enemyProjectile.Launch(direction);
        FMODManager.PlayEnemyShot(position);
    }

    bool IsVisible()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null) return true;

        if (!TryGetBodyBounds(out Bounds bounds)) return false;
        Vector3 min = gameCamera.WorldToViewportPoint(bounds.min);
        Vector3 max = gameCamera.WorldToViewportPoint(bounds.max);
        return max.z > 0f && max.x >= 0f && min.x <= 1f && max.y >= 0f && min.y <= 1f;
    }

    bool IsEntireBodyBelowScreen()
    {
        if (gameCamera == null || renderers.Length == 0) return false;
        if (!TryGetBodyBounds(out Bounds bounds)) return false;
        return gameCamera.WorldToViewportPoint(bounds.max).y < -0.15f;
    }

    bool TryGetBodyBounds(out Bounds bounds)
    {
        bounds = default;
        bool found = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            if (!found)
            {
                bounds = renderers[i].bounds;
                found = true;
            }
            else bounds.Encapsulate(renderers[i].bounds);
        }
        return found;
    }

    public void FlashDamage()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].color = new Color(1f, 0.55f, 0.55f, 1f);
        }
        damageFlashEndsAt = Time.time + 0.06f;
    }

    void RestoreColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].color = normalColors[i];
        }
        damageFlashEndsAt = 0f;
    }

    void OnTriggerEnter2D(Collider2D other) => TryDamagePlayer(other);
    void OnTriggerStay2D(Collider2D other) => TryDamagePlayer(other);

    void TryDamagePlayer(Collider2D other)
    {
        if (Time.time < nextContactDamageTime) return;

        Health playerHealth = other.GetComponentInParent<Health>();
        if (playerHealth == null || !playerHealth.CompareTag("Player")) return;

        nextContactDamageTime = Time.time + contactDamageCooldown;
        playerHealth.TakeDamage(contactDamage);
    }

    void OnDisable()
    {
        if (renderers != null && normalColors != null) RestoreColors();
    }
}
