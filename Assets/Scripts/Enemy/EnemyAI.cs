using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3f;
    public GameObject projectilePrefab;
    public Transform firePoint; // Agora configurável diretamente no inspector
    public float fireRate = 1f;
    private float nextFire;

    public int maxHits = 2; // Quantidade de hits para destruir o inimigo
    private EnemyHealth enemyHealth; // Referência ao script de vida do inimigo

    void Start()
    {
        // Obtém a referência ao script EnemyHealth no início
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        Move();

        if (Time.time > nextFire && IsWithinScreenBounds())
        {
            Shoot();
            nextFire = Time.time + fireRate;
        }
    }

    bool IsWithinScreenBounds()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        return screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
    }

    void Move()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    void Shoot()
    {
        if (firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            Debug.LogWarning("FirePoint não está configurado.");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Colidiu com: {collision.gameObject.name}"); // Log do objeto que colidiu

        if (collision.gameObject.CompareTag("Player"))
        {
            // Causa dano ao jogador se ele colidir com o inimigo
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }
            Destroy(gameObject); // Destroi o inimigo após a colisão
        }
        else if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            Debug.Log("Projétil do jogador detectado!");
            
            // Aplica dano ao inimigo
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(10); // Dano de 10
            }

            // Destroi o projétil do jogador após a colisão
            Destroy(collision.gameObject);
        }
    }
}
