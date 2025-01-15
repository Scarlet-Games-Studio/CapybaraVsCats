using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public int damage = 10;
    public GameObject explosionPrefab;

    void Start()
    {
        Invoke("DestroyProjectile", lifeTime);
    }

    void Update()
    {
        if (gameObject.CompareTag("PlayerProjectile"))
        {
            MoveUp();
        }
        else
        {
            MoveDown();
        }

        if (!IsWithinScreenBounds())
        {
            DestroyProjectile();
        }
    }

    bool IsWithinScreenBounds()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        return screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
    }

    void MoveUp()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void MoveDown()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.CompareTag("PlayerProjectile"))
        {
            if (collision.CompareTag("Enemy"))
            {
                SpawnExplosion();
                Destroy(collision.gameObject);
                Destroy(gameObject);
            }

            if (collision.CompareTag("Boss"))
            {
                BossG1BulletPattern bossScript = collision.GetComponent<BossG1BulletPattern>();
                if (bossScript != null)
                {
                    bossScript.TakeDamage(damage);
                }
                SpawnExplosion();
                Destroy(gameObject);
            }
        }

        if (gameObject.CompareTag("EnemyProjectile"))
        {
            if (collision.CompareTag("Player"))
            {
                Health playerHealth = collision.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
                SpawnExplosion();
                Destroy(gameObject);
            }
        }
    }

    void SpawnExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, GetAnimationClipLength(explosion));
        }
    }

    void DestroyProjectile()
    {
        SpawnExplosion();
        Destroy(gameObject);
    }

    float GetAnimationClipLength(GameObject explosion)
    {
        Animator animator = explosion.GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                return clipInfo[0].clip.length; // Retorna a duração do primeiro clipe da animação
            }
        }
        return 0.5f; // Valor padrão se a animação não for encontrada
    }
}
