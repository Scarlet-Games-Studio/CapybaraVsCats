using System.Collections;
using UnityEngine;

public class BossG1BulletPattern : MonoBehaviour
{
    [Header("Bullet Pattern")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public int bulletCount = 12;
    public float fireRate = 0.5f;
    private float nextFireTime;
    private bool isVisible = false;
    private float spiralAngle = 0f;

    // Variáveis do Boss
    [Header("Boss stats & states")]
    public int health = 300; // Vida inicial do boss
    public GameObject deathEffect; // Efeito de morte do boss (opcional)
    private bool isDead = false;

    [Header("Animation")]
    public SpriteRenderer sr;
    public Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    void Update()
    {
        if(health >= 200)
        {
            if (isVisible && Time.time >= nextFireTime)
            {
                FireUniquePattern();
                nextFireTime = Time.time + fireRate;
            }
        }
        else if(health < 200)
        {

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
        {
            FireExpandingCirclePattern();
        }

        else
        {
            FireSpiralBurstPattern();
        }
    }

    void FireSkillShotPattern()
    {

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
        StartCoroutine(FireSpiralBurst());
    }

    IEnumerator FireSpiralBurst()
    {
        float angleStep = 20f;
        for (int i = 0; i < bulletCount; i++)
        {
            float bulletDirX = Mathf.Sin(spiralAngle * Mathf.Deg2Rad);
            float bulletDirY = Mathf.Cos(spiralAngle * Mathf.Deg2Rad);

            Vector2 bulletDirection = new Vector2(bulletDirX, bulletDirY).normalized;

            SpawnBullet(bulletDirection);

            spiralAngle = (spiralAngle + angleStep) % 360f;

            yield return new WaitForSeconds(0.1f);
        }
    }

    void SpawnBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * bulletSpeed;
        }

        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true;
        }

        Destroy(bullet, 5f); // Destroi o projétil após 5 segundos
    }

    // Método para o boss receber dano
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        StartCoroutine(Flashing());

        // Verifica se a vida chegou a 0 ou menos
        if (health <= 0)
        {
            Die();
        }
    }

    // Lógica de morte do boss
    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    //Faz o efeito de brilho quando leva dano
    IEnumerator Flashing()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.03f);
        sr.color = Color.white;
    }



    //Essa função está sendo removida, pois o tiro do player já faz isso

    // Método para detectar colisões com projéteis
    /*void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerProjectile"))
        {
            Projectile projectile = collision.GetComponent<Projectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
                Destroy(collision.gameObject);
            }
            else
            {
                TakeDamage(10); // Valor fixo caso não tenha script Projectile
                Destroy(collision.gameObject);
            }
        }
    }*/
}
