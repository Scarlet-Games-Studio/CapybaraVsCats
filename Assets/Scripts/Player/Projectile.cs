using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;
    public int damage = 10;

    void Start()
    {
        Invoke("DestroyProjectile", lifeTime);
    }

    void Update()
    {
        // se a tag do projétil for PlayerProjectile
        if (gameObject.CompareTag("PlayerProjectile"))
        {
            MoveUp();
        }
        else
        {
            MoveDown();
        }

        // Destrua o projétil se ele estiver fora dos limites da tela
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
        // se a tag do projétil for PlayerProjectile
        if (gameObject.CompareTag("PlayerProjectile"))
        {
            if (collision.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
                Destroy(gameObject);
            }

            if (collision.CompareTag("Boss"))
            {
                BossG1BulletPattern bossScript = collision.GetComponent<BossG1BulletPattern>();
                if (bossScript != null)
                {
                    bossScript.TakeDamage(damage); // Aplica o dano ao boss
                }
                Destroy(gameObject); // Destrói o projétil
            }
        }

        // se a tag do projétil for EnemyProjectile
        if (gameObject.CompareTag("EnemyProjectile"))
        {
            if (collision.CompareTag("Player"))
            {
                Health playerHealth = collision.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
                Destroy(gameObject);
            }
        }
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
