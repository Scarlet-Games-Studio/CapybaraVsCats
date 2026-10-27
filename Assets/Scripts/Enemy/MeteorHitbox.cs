using System.Collections;
using UnityEngine;

public class MeteorHitbox : MonoBehaviour
{
    [SerializeField, Min(1)] int meteorHealth = 4;
    [SerializeField, Min(0)] int playerContactDamage = 10;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Animator animator;

    [Header("Idle animation")]
    [SerializeField] bool animateIdle = true;
    [SerializeField, Min(0f)] float tumbleDegreesPerSecond = 18f;
    [SerializeField, Min(0f)] float bobHeight = 0.12f;
    [SerializeField, Min(0f)] float bobCyclesPerSecond = 0.35f;

    bool exploding;
    bool flashing;
    int explodingParameter;
    Vector3 idleStartPosition;
    Quaternion idleStartRotation;
    float idleTime;

    void Awake()
    {
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        idleStartPosition = transform.localPosition;
        idleStartRotation = transform.localRotation;
        explodingParameter = Animator.StringToHash("isExploding");

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.constraints = RigidbodyConstraints2D.None;
        }
    }

    void Update()
    {
        if (!animateIdle || exploding) return;

        idleTime += Time.deltaTime;
        float phase = idleTime * Mathf.PI * 2f * bobCyclesPerSecond;
        transform.localPosition = idleStartPosition + Vector3.up * (Mathf.Sin(phase) * bobHeight);
        transform.localRotation = idleStartRotation * Quaternion.Euler(0f, 0f, idleTime * tumbleDegreesPerSecond);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (exploding || collision == null) return;

        if (collision.CompareTag("PlayerProjectile"))
        {
            meteorHealth = Mathf.Max(0, meteorHealth - 1);
            Projectile projectile = collision.GetComponent<Projectile>();
            if (projectile != null) projectile.ConsumeOnObstacle();
            if (meteorHealth == 0) BeginExplosion();
            else if (!flashing && sr != null) StartCoroutine(FlashOnce());
            return;
        }

        Health playerHealth = collision.GetComponentInParent<Health>();
        if (playerHealth != null && playerHealth.CompareTag("Player"))
        {
            if (playerContactDamage > 0) playerHealth.TakeDamage(playerContactDamage);
            BeginExplosion();
        }
    }

    void BeginExplosion()
    {
        if (exploding) return;
        exploding = true;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D meteorCollider in colliders) meteorCollider.enabled = false;

        if (animator != null && animator.runtimeAnimatorController != null && HasExplodingParameter())
            animator.SetBool(explodingParameter, true);
        else
            Destroy(gameObject, 0.25f);
    }

    bool HasExplodingParameter()
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == explodingParameter && parameter.type == AnimatorControllerParameterType.Bool)
                return true;
        }
        return false;
    }

    IEnumerator FlashOnce()
    {
        flashing = true;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.06f);
        if (sr != null && !exploding) sr.color = Color.white;
        flashing = false;
    }
}
