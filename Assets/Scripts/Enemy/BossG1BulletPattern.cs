using System.Collections;
using UnityEngine;

public class BossG1BulletPattern : MonoBehaviour
{
    [Header("Screen bounds")]
    [SerializeField, Range(0f, 0.25f)] private float viewportPadding = 0.08f;

    [Header("Horizontal movement")]
    [SerializeField, Range(0.05f, 0.4f)] private float horizontalRange = 0.18f;
    [SerializeField, Min(0.1f)] private float horizontalSmoothTime = 0.65f;
    [SerializeField] private Vector2 directionChangeInterval = new Vector2(1.5f, 3f);
    private float baseViewportX;
    private float targetViewportX;
    private float horizontalVelocity;
    private float nextDirectionChange;

    [Header("Bullet Pattern")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;
    public int bulletCount = 12;
    public float fireRate = 0.5f;
    [SerializeField]private float nextFireTime;
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
    public GameObject VFX;
    public GameObject SkillShot;


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

        Camera gameCamera = Camera.main;
        baseViewportX = gameCamera != null
            ? gameCamera.WorldToViewportPoint(transform.position).x
            : 0.5f;
        targetViewportX = baseViewportX;
        ScheduleHorizontalMove();
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
            animator.SetBool("AttackWindUp", true);
            FireSkillShotPattern();
        }
    }

    void LateUpdate()
    {
        // Algumas animações antigas do boss possuem curvas de Transform. Mantém o
        // objeto dentro da área jogável mesmo que uma dessas curvas mova a raiz.
        Camera gameCamera = Camera.main;
        if (gameCamera == null) return;

        Vector3 viewportPosition = gameCamera.WorldToViewportPoint(transform.position);
        if (viewportPosition.z < 0f) return;

        if (Time.time >= nextDirectionChange)
            ScheduleHorizontalMove();

        viewportPosition.x = Mathf.SmoothDamp(
            viewportPosition.x,
            targetViewportX,
            ref horizontalVelocity,
            horizontalSmoothTime);

        viewportPosition.x = Mathf.Clamp(viewportPosition.x, viewportPadding, 1f - viewportPadding);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, viewportPadding, 1f - viewportPadding);
        transform.position = gameCamera.ViewportToWorldPoint(viewportPosition);
    }

    void ScheduleHorizontalMove()
    {
        float minimumX = Mathf.Max(viewportPadding, baseViewportX - horizontalRange);
        float maximumX = Mathf.Min(1f - viewportPadding, baseViewportX + horizontalRange);
        targetViewportX = Random.Range(minimumX, maximumX);
        nextDirectionChange = Time.time + Random.Range(
            Mathf.Min(directionChangeInterval.x, directionChangeInterval.y),
            Mathf.Max(directionChangeInterval.x, directionChangeInterval.y));
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
        FireExpandingCirclePattern();
        FireSpiralBurstPattern();
    }

    void FireSkillShotPattern()
    {
        if(animator.GetBool("AttackReleased"))
        {
            StartCoroutine(Wait());
        }
    }

    //Espera faz com que só seja spawnado apenas 1 projetil e efeito VFX
    IEnumerator Wait()
    {
        Instantiate(VFX, transform.position, Quaternion.identity);
        Instantiate(SkillShot, transform.position, Quaternion.identity);
        yield return 0;
        animator.SetBool("AttackReleased", false);
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
