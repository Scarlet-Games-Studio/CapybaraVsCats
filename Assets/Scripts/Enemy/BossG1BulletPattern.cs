using System.Collections;
using UnityEngine;

public class BossG1BulletPattern : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public int bulletCount = 12;
    public float fireRate = 0.5f;
    private float nextFireTime;
    private bool isVisible = false;
    private float spiralAngle = 0f;

    // Variáveis do Boss
    public int health = 100; // Vida inicial do boss
    public GameObject deathEffect; // Efeito de morte do boss (opcional)

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void Update()
    {
        if (isVisible && Time.time >= nextFireTime)
        {
            FireUniquePattern();
            nextFireTime = Time.time + fireRate;
        }
    }

    void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
    }

    void FireUniquePattern()
    {
        int randomPattern = Random.Range(0, 2);

        if (randomPattern == 0)
            FireExpandingCirclePattern();
        else
            FireSpiralBurstPattern();
    }

    void FireExpandingCirclePattern()
    {
        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float bulletDirX = Mathf.Sin(angle * Mathf.Deg2Rad);
            float bulletDirY = Mathf.Cos(angle * Mathf.Deg2Rad);

            Vector2 bulletDirection = new Vector2(bulletDirX, bulletDirY).normalized;

            SpawnBullet(bulletDirection);

            angle += angleStep;
        }
    }

    void FireSpiralBurstPattern()
    {
        float angleStep = 20f;
        for (int i = 0; i < bulletCount; i++)
        {
            float bulletDirX = Mathf.Sin(spiralAngle * Mathf.Deg2Rad);
            float bulletDirY = Mathf.Cos(spiralAngle * Mathf.Deg2Rad);

            Vector2 bulletDirection = new Vector2(bulletDirX, bulletDirY).normalized;

            SpawnBullet(bulletDirection);

            spiralAngle += angleStep;
        }
    }

    void SpawnBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * bulletSpeed;
            bulletRb.isKinematic = true;
        }

        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true;
        }
    }

    // Método para o boss receber dano
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Verifica se a vida chegou a 0 ou menos
        if (health <= 0)
        {
            Die();
        }
    }

    // Lógica de morte do boss
    void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    // Método para detectar colisões com projéteis
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerProjectile"))
        {
            Projectile projectile = collision.GetComponent<Projectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
                Destroy(collision.gameObject);
            }
        }
    }
}
