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
        // Chama DestroyProjectile após o tempo de vida do projétil (lifeTime)
        Invoke("DestroyProjectile", lifeTime);
    }

    void Update()
    {
        // Move o projétil dependendo de sua tag
        if (gameObject.CompareTag("PlayerProjectile"))
        {
            MoveUp();
        }
        else
        {
            MoveDown();
        }

        // Verifica se o projétil saiu da tela e o destrói
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
        // Se o projétil colidir com um inimigo ou boss, cria a explosão e destrói o projétil
        if (gameObject.CompareTag("PlayerProjectile"))
        {
            if (collision.CompareTag("Enemy"))
            {
                SpawnExplosion();
                Destroy(collision.gameObject); // Destroi o inimigo
                Destroy(gameObject); // Destroi o projétil
            }

            if (collision.CompareTag("Boss"))
            {
                BossG1BulletPattern bossScript = collision.GetComponent<BossG1BulletPattern>();
                if (bossScript != null)
                {
                    bossScript.TakeDamage(damage);
                }
                SpawnExplosion();
                Destroy(gameObject); // Destroi o projétil
            }
        }

        // Se o projétil for do inimigo e colidir com o jogador, causa dano e gera a explosão
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
                Destroy(gameObject); // Destroi o projétil
            }
        }
    }

    // Método para instanciar a explosão
    void SpawnExplosion()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            // Destrói a explosão após o tempo da animação terminar
            Destroy(explosion, GetAnimationClipLength(explosion));
        }
    }

    // Método que destrói o projétil após o tempo de vida
    void DestroyProjectile()
    {
        SpawnExplosion();
        Destroy(gameObject); // Destroi o projétil após o tempo de vida
    }

    // Método para pegar o tempo da animação da explosão
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
