using System;
using System.Collections;
using UnityEngine;

public class BossG1BulletPattern : MonoBehaviour
{
    [Header("Screen bounds")]
    [SerializeField, Range(0.02f, 0.25f)] float viewportPadding = 0.1f;
    [SerializeField, Range(0.55f, 0.95f)] float targetViewportY = 0.78f;
    [SerializeField, Min(0.05f)] float entrySpeed = 0.45f;
    [SerializeField, Min(0f)] float verticalBobAmount = 0.018f;
    [SerializeField, Min(0f)] float verticalBobSpeed = 1.4f;

    [Header("Horizontal movement")]
    [SerializeField, Range(0.05f, 0.4f)] float horizontalRange = 0.22f;
    [SerializeField, Min(0.1f)] float horizontalSmoothTime = 0.55f;
    [SerializeField] Vector2 directionChangeInterval = new Vector2(1.3f, 2.6f);

    [Header("Bullet Pattern")]
    public GameObject bulletPrefab;
    [Min(0.1f)] public float bulletSpeed = 5f;
    [Min(1)] public int bulletCount = 12;
    [Min(0.1f)] public float fireRate = 0.5f;
    [SerializeField] float nextFireTime;
    [SerializeField, Min(0.25f)] float skillShotCooldown = 1.5f;

    [Header("Boss stats & states")]
    [Min(1)] public int health = 300;
    [SerializeField, Range(0.1f, 0.9f)] float phaseTwoHealthRatio = 0.4f;
    public GameObject deathEffect;

    [Header("Animation")]
    public SpriteRenderer sr;
    public Animator animator;
    public GameObject VFX;
    public GameObject SkillShot;

    public bool IsDead { get; private set; }
    public int CurrentHealth => Mathf.Max(0, health);
    public int MaximumHealth => Mathf.Max(1, maximumHealth);
    public event Action Died;
    public event Action<int, int> HealthChanged;

    Camera gameCamera;
    Coroutine spiralRoutine;
    int maximumHealth;
    float baseViewportX;
    float currentViewportX;
    float currentViewportY;
    float targetViewportX;
    float horizontalVelocity;
    float nextDirectionChange;
    float nextSkillShotTime;
    float cameraDepth;
    bool hasWindUpParameter;
    bool hasReleasedParameter;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        hasWindUpParameter = HasAnimatorParameter("AttackWindUp");
        hasReleasedParameter = HasAnimatorParameter("AttackReleased");
        maximumHealth = Mathf.Max(1, health);

        if (GetComponent<BossHealthBarController>() == null)
            gameObject.AddComponent<BossHealthBarController>();

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void Start()
    {
        FMODManager.PlayBossMusic();
        gameCamera = Camera.main;
        if (gameCamera == null)
        {
            Debug.LogError("Boss não encontrou a câmera principal.", this);
            enabled = false;
            return;
        }

        cameraDepth = Mathf.Abs(transform.position.z - gameCamera.transform.position.z);
        Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
        baseViewportX = Mathf.Clamp(viewport.x, viewportPadding, 1f - viewportPadding);
        currentViewportX = baseViewportX;
        currentViewportY = Mathf.Clamp(viewport.y, viewportPadding, 1f - viewportPadding);
        targetViewportX = baseViewportX;
        ScheduleHorizontalMove();
        nextFireTime = Time.time + 0.75f;
    }

    void Update()
    {
        if (IsDead || (GameManager.instance != null && !GameManager.instance.IsPlaying)) return;
        if (!IsInsideCamera()) return;

        bool phaseTwo = health <= Mathf.CeilToInt(maximumHealth * phaseTwoHealthRatio);
        if (hasWindUpParameter) animator.SetBool("AttackWindUp", phaseTwo);

        if (!phaseTwo)
        {
            if (Time.time < nextFireTime) return;
            FireExpandingCirclePattern();
            if (spiralRoutine == null) spiralRoutine = StartCoroutine(FireSpiralBurst());
            nextFireTime = Time.time + Mathf.Max(0.1f, fireRate);
        }
        else if (Time.time >= nextSkillShotTime && ReadyToReleaseSkill())
        {
            FireSkillShot();
            nextSkillShotTime = Time.time + skillShotCooldown;
        }
    }

    void LateUpdate()
    {
        if (gameCamera == null || IsDead) return;

        if (Time.time >= nextDirectionChange) ScheduleHorizontalMove();
        currentViewportX = Mathf.SmoothDamp(currentViewportX, targetViewportX, ref horizontalVelocity, horizontalSmoothTime);
        currentViewportX = Mathf.Clamp(currentViewportX, viewportPadding, 1f - viewportPadding);
        currentViewportY = Mathf.MoveTowards(currentViewportY, targetViewportY, entrySpeed * Time.deltaTime);

        float bob = Mathf.Sin(Time.time * verticalBobSpeed) * verticalBobAmount;
        float y = Mathf.Clamp(currentViewportY + bob, viewportPadding, 1f - viewportPadding);
        transform.position = gameCamera.ViewportToWorldPoint(new Vector3(currentViewportX, y, cameraDepth));
    }

    void ScheduleHorizontalMove()
    {
        float minimumX = Mathf.Max(viewportPadding, baseViewportX - horizontalRange);
        float maximumX = Mathf.Min(1f - viewportPadding, baseViewportX + horizontalRange);
        targetViewportX = UnityEngine.Random.Range(minimumX, maximumX);
        float minTime = Mathf.Max(0.1f, Mathf.Min(directionChangeInterval.x, directionChangeInterval.y));
        float maxTime = Mathf.Max(minTime, Mathf.Max(directionChangeInterval.x, directionChangeInterval.y));
        nextDirectionChange = Time.time + UnityEngine.Random.Range(minTime, maxTime);
    }

    bool IsInsideCamera()
    {
        return currentViewportX >= 0f && currentViewportX <= 1f && currentViewportY >= 0f && currentViewportY <= 1f;
    }

    bool ReadyToReleaseSkill()
    {
        return !hasReleasedParameter || animator.GetBool("AttackReleased");
    }

    void FireSkillShot()
    {
        FMODManager.PlayBossWeapon(transform.position);
        if (VFX != null) Instantiate(VFX, transform.position, Quaternion.identity);
        if (SkillShot != null) Instantiate(SkillShot, transform.position, Quaternion.identity);
        if (hasReleasedParameter) animator.SetBool("AttackReleased", false);
    }

    bool HasAnimatorParameter(string parameterName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter parameter in animator.parameters)
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == parameterName)
                return true;
        return false;
    }

    void FireExpandingCirclePattern()
    {
        if (bulletPrefab == null) return;
        FMODManager.PlayBossWeapon(transform.position);
        int count = Mathf.Max(1, bulletCount);
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            SpawnBullet(new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized);
        }
    }

    IEnumerator FireSpiralBurst()
    {
        if (bulletPrefab == null)
        {
            spiralRoutine = null;
            yield break;
        }

        int count = Mathf.Max(1, bulletCount);
        FMODManager.PlayBossWeapon(transform.position);
        float angle = UnityEngine.Random.Range(0f, 360f);
        for (int i = 0; i < count && !IsDead; i++)
        {
            float radians = angle * Mathf.Deg2Rad;
            SpawnBullet(new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)).normalized);
            angle = (angle + 20f) % 360f;
            yield return new WaitForSeconds(0.1f);
        }
        spiralRoutine = null;
    }

    void SpawnBullet(Vector2 direction)
    {
        if (bulletPrefab == null) return;
        GameObject bullet = ProjectileSpawnLimiter.Spawn(bulletPrefab, transform.position, Quaternion.identity);
        if (bullet == null) return;
        Rigidbody2D bulletBody = bullet.GetComponent<Rigidbody2D>();
        if (bulletBody != null) bulletBody.linearVelocity = direction * bulletSpeed;

        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null) bulletCollider.isTrigger = true;
        Destroy(bullet, 5f);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return;
        int previousHealth = health;
        health = Mathf.Max(0, health - damage);
        if (health != previousHealth) HealthChanged?.Invoke(health, maximumHealth);
        if (health == 0) Die();
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        FMODManager.StopBossWeapon();
        FMODManager.PlayEnemyDeath(transform.position);
        if (spiralRoutine != null) StopCoroutine(spiralRoutine);
        Died?.Invoke();

        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);

        BossDeath deathHandler = GetComponent<BossDeath>();
        if (deathHandler != null)
            deathHandler.NotifyDefeated();
        else
        {
            ScoreManager.AddScore(1500);
            if (GameManager.instance != null) GameManager.instance.CompleteStage();
            else FindAnyObjectByType<StageManager>(FindObjectsInactive.Include)?.OnStageComplete();
        }

        Destroy(gameObject);
    }

    void OnDisable()
    {
        FMODManager.StopBossWeapon();
    }

}

[DisallowMultipleComponent]
public class BossHealthBarController : MonoBehaviour
{
    const int FrameSortingOrder = 70;
    const int FillSortingOrder = 71;
    const int FullLifeSortingOrder = 72;
    const int DamageSortingOrder = 73;

    [SerializeField, Min(0.1f)] float drainSpeed = 1.8f;
    [SerializeField, Min(0.05f)] float damageVfxDuration = 0.45f;
    [SerializeField, Min(0.03f)] float flipInterval = 0.08f;

    BossG1BulletPattern boss;
    Transform barRoot;
    SpriteRenderer frameRenderer;
    SpriteRenderer fillRenderer;
    SpriteRenderer fullLifeRenderer;
    SpriteRenderer damageRenderer;

    Vector3 initialFillScale;
    Vector3 initialFillPosition;
    Vector3 initialDamagePosition;
    float fillBoundsMinX;
    float fillBoundsMaxX;
    float fillLeftEdge;
    float damageOffsetFromFillEdge;
    float displayedRatio = 1f;
    float targetRatio = 1f;
    float damageTimer;
    float flipTimer;
    int lastHealth;
    bool initialDamageFlipY;
    bool initialized;

    void Awake()
    {
        boss = GetComponent<BossG1BulletPattern>();
        FindBarParts();
    }

    void OnEnable()
    {
        if (boss == null) boss = GetComponent<BossG1BulletPattern>();
        if (boss != null) boss.HealthChanged += OnHealthChanged;
    }

    void Start()
    {
        if (!initialized) InitializeBar();
    }

    void OnDisable()
    {
        if (boss != null) boss.HealthChanged -= OnHealthChanged;
    }

    void Update()
    {
        if (!initialized) return;

        if (!Mathf.Approximately(displayedRatio, targetRatio))
        {
            displayedRatio = Mathf.MoveTowards(displayedRatio, targetRatio, drainSpeed * Time.deltaTime);
            ApplyFill(displayedRatio);
        }

        UpdateDamageEffect();
    }

    void FindBarParts()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer candidate in renderers)
        {
            string key = Normalize(candidate.gameObject.name);
            if (key == "bosshpbar")
            {
                barRoot = candidate.transform;
                frameRenderer = candidate;
            }
            else if (key == "filledfulllife" || key == "fillfulllife" || key == "fulllife")
                fullLifeRenderer = candidate;
            else if (key == "filllife" || key == "lifefill" || key == "healthfill" || key == "hpfill")
                fillRenderer = candidate;
            else if (key.Contains("damage") &&
                     (key.Contains("inbar") || key.Contains("lifedrain") || key.Contains("vfx")))
                damageRenderer = candidate;
        }
    }

    void InitializeBar()
    {
        if (boss == null || fillRenderer == null)
        {
            Debug.LogWarning("A HP Bar do boss não encontrou o BossG1BulletPattern ou o sprite Fill Life.", this);
            enabled = false;
            return;
        }

        if (barRoot != null) barRoot.gameObject.SetActive(true);
        PrepareRenderer(frameRenderer, FrameSortingOrder);
        PrepareRenderer(fillRenderer, FillSortingOrder);
        PrepareRenderer(fullLifeRenderer, FullLifeSortingOrder);
        PrepareRenderer(damageRenderer, DamageSortingOrder);

        initialFillScale = fillRenderer.transform.localScale;
        initialFillPosition = fillRenderer.transform.localPosition;
        if (fillRenderer.sprite != null)
        {
            fillBoundsMinX = fillRenderer.sprite.bounds.min.x;
            fillBoundsMaxX = fillRenderer.sprite.bounds.max.x;
        }

        float fullWidth = Mathf.Abs((fillBoundsMaxX - fillBoundsMinX) * initialFillScale.x);
        fillLeftEdge = initialFillPosition.x + ScaledLeftOffset(initialFillScale.x);
        float initialRightEdge = fillLeftEdge + fullWidth;

        if (damageRenderer != null)
        {
            initialDamagePosition = damageRenderer.transform.localPosition;
            damageOffsetFromFillEdge = initialDamagePosition.x - initialRightEdge;
            initialDamageFlipY = damageRenderer.flipY;
            damageRenderer.enabled = false;
        }

        lastHealth = boss.CurrentHealth;
        displayedRatio = targetRatio = Mathf.Clamp01((float)boss.CurrentHealth / boss.MaximumHealth);
        if (fullLifeRenderer != null)
        {
            fullLifeRenderer.enabled = displayedRatio >= 0.999f;
            fillRenderer.enabled = displayedRatio < 0.999f && displayedRatio > 0f;
        }
        else
            fillRenderer.enabled = displayedRatio > 0f;

        ApplyFill(displayedRatio);
        initialized = true;
    }

    void OnHealthChanged(int currentHealth, int maximumHealth)
    {
        if (!initialized) InitializeBar();
        if (!initialized) return;

        targetRatio = maximumHealth > 0 ? Mathf.Clamp01((float)currentHealth / maximumHealth) : 0f;
        bool tookDamage = currentHealth < lastHealth;
        lastHealth = currentHealth;

        if (fullLifeRenderer != null && targetRatio < 0.999f)
        {
            fullLifeRenderer.enabled = false;
            fillRenderer.enabled = targetRatio > 0f;
        }

        if (tookDamage && damageRenderer != null && targetRatio > 0f)
        {
            damageTimer = damageVfxDuration;
            flipTimer = 0f;
            damageRenderer.enabled = true;
        }
    }

    void ApplyFill(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        Vector3 scale = initialFillScale;
        scale.x = initialFillScale.x * ratio;
        fillRenderer.transform.localScale = scale;

        Vector3 position = initialFillPosition;
        float currentWidth = Mathf.Abs((fillBoundsMaxX - fillBoundsMinX) * scale.x);
        position.x = fillLeftEdge - ScaledLeftOffset(scale.x);
        fillRenderer.transform.localPosition = position;
        fillRenderer.enabled = ratio > 0f && (fullLifeRenderer == null || !fullLifeRenderer.enabled);

        if (damageRenderer != null)
        {
            Vector3 damagePosition = initialDamagePosition;
            damagePosition.x = fillLeftEdge + currentWidth + damageOffsetFromFillEdge;
            damageRenderer.transform.localPosition = damagePosition;
        }
    }

    void UpdateDamageEffect()
    {
        if (damageRenderer == null || !damageRenderer.enabled) return;

        damageTimer -= Time.deltaTime;
        flipTimer -= Time.deltaTime;
        if (flipTimer <= 0f)
        {
            flipTimer = flipInterval;
            damageRenderer.flipY = !damageRenderer.flipY;
        }

        if (damageTimer > 0f) return;
        damageRenderer.flipY = initialDamageFlipY;
        damageRenderer.enabled = false;
    }

    static void PrepareRenderer(SpriteRenderer renderer, int sortingOrder)
    {
        if (renderer == null) return;
        renderer.sortingLayerName = "Characters";
        renderer.sortingOrder = sortingOrder;
        renderer.forceRenderingOff = false;
    }

    float ScaledLeftOffset(float scaleX)
    {
        return scaleX >= 0f ? fillBoundsMinX * scaleX : fillBoundsMaxX * scaleX;
    }

    static string Normalize(string value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
    }
}
