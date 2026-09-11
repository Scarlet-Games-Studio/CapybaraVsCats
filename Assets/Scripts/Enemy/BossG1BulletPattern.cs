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
    public event Action Died;

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
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletBody = bullet.GetComponent<Rigidbody2D>();
        if (bulletBody != null) bulletBody.linearVelocity = direction * bulletSpeed;

        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null) bulletCollider.isTrigger = true;
        Destroy(bullet, 5f);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return;
        health = Mathf.Max(0, health - damage);
        if (sr != null) StartCoroutine(Flashing());
        if (health == 0) Die();
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;
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

    IEnumerator Flashing()
    {
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        if (sr != null) sr.color = original;
    }
}
