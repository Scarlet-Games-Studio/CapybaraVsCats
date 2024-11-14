using System.Collections;
using UnityEngine;

public class BossG1BulletPattern : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab do projétil
    public float bulletSpeed = 5f; // Velocidade dos projéteis
    public int bulletCount = 12; // Número de projéteis por padrão de disparo
    public float fireRate = 0.5f; // Frequência de disparo do BossG1
    private float nextFireTime;
    private bool isVisible = false; // Variável para verificar se o Boss está visível

    void Start()
    {
        // Garantindo que o BossG1 fique parado
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
        // Dispara apenas se o Boss estiver visível e for hora de disparar
        if (isVisible && Time.time >= nextFireTime)
        {
            FireUniquePattern();
            nextFireTime = Time.time + fireRate;
        }
    }

    // Método Unity chamado quando o objeto se torna visível na câmera
    void OnBecameVisible()
    {
        isVisible = true; // Marca o Boss como visível
    }

    // Método Unity chamado quando o objeto deixa de ser visível na câmera
    void OnBecameInvisible()
    {
        isVisible = false; // Marca o Boss como invisível
    }

    // Padrão único para o BossG1
    void FireUniquePattern()
    {
        // Alterna entre dois padrões para o BossG1
        int randomPattern = Random.Range(0, 2);

        if (randomPattern == 0)
            FireExpandingCirclePattern();
        else
            FireSpiralBurstPattern();
    }

    // Padrão circular que se expande
    void FireExpandingCirclePattern()
    {
        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            // Define a direção de cada projétil
            float bulletDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180);
            float bulletDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180);

            Vector3 bulletMoveVector = new Vector3(bulletDirX, bulletDirY, 0f);
            Vector2 bulletDirection = (bulletMoveVector - transform.position).normalized;

            // Instancia e dispara o projétil
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bulletDirection * bulletSpeed;
            }

            angle += angleStep;
        }
    }

    // Padrão em espiral com "rajadas" rápidas
    void FireSpiralBurstPattern()
    {
        float angleStep = 20f;
        float angle = Time.time * 100; // Ajuste para criar efeito de espiral

        for (int i = 0; i < bulletCount; i++)
        {
            float bulletDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180);
            float bulletDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180);

            Vector3 bulletMoveVector = new Vector3(bulletDirX, bulletDirY, 0f);
            Vector2 bulletDirection = (bulletMoveVector - transform.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = bulletDirection * bulletSpeed;
            }

            angle += angleStep;
        }
    }
}
