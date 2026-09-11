using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Min(0.1f)] public float speed = 10f;
    [Min(0.1f)] public float lifeTime = 2f;
    [Min(1)] public int damage = 10;
    public GameObject explosionPrefab;

    Camera gameCamera;
    bool consumed;

    void Start()
    {
        gameCamera = Camera.main;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        Vector2 direction = CompareTag("PlayerProjectile") ? Vector2.up : Vector2.down;
        transform.Translate(direction * speed * Time.deltaTime, Space.Self);

        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera != null && IsFarOutsideScreen()) Destroy(gameObject);
    }

    bool IsFarOutsideScreen()
    {
        Vector3 viewport = gameCamera.WorldToViewportPoint(transform.position);
        return viewport.z <= 0f || viewport.x < -0.15f || viewport.x > 1.15f || viewport.y < -0.15f || viewport.y > 1.15f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (consumed) return;

        if (CompareTag("PlayerProjectile"))
        {
            BossG1BulletPattern boss = collision.GetComponentInParent<BossG1BulletPattern>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                Consume();
                return;
            }

            EnemyHealth enemy = collision.GetComponentInParent<EnemyHealth>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(damage);
                collision.GetComponentInParent<EnemyAI>()?.FlashDamage();
                Consume();
            }
            return;
        }

        if (CompareTag("EnemyProjectile"))
        {
            Health playerHealth = collision.GetComponentInParent<Health>();
            if (playerHealth != null && playerHealth.CompareTag("Player"))
            {
                playerHealth.TakeDamage(damage);
                Consume();
            }
        }
    }

    void Consume()
    {
        if (consumed) return;
        consumed = true;
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, GetAnimationClipLength(explosion));
        }
        Destroy(gameObject);
    }

    static float GetAnimationClipLength(GameObject explosion)
    {
        Animator animator = explosion.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips.Length > 0) return Mathf.Max(0.1f, clips[0].length);
        }
        return 0.5f;
    }
}
