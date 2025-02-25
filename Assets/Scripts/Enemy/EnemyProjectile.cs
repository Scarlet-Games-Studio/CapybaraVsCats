using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f; // Velocidade do projétil
    public float lifeTime = 2f; // Duração do projétil antes de ser destruído
    public int damage = 10; // Dano causado pelo projétil
    private Vector2 direction; // Direção fixa do projétil
    [SerializeField] bool bossProjectile = false;

    void Start()
    {
        // Define a direção inicial como para baixo
        direction = Vector2.down;

        // Destroi o projétil automaticamente após 'lifeTime' segundos
        if (bossProjectile)
        {
            Invoke("DestroyProjectile", lifeTime);
        }
    }

    void Update()
    {
        // Move o projétil na direção fixa
        MoveProjectile();

        // Destroi o projétil se sair da tela
        if (!IsWithinScreenBounds() && !bossProjectile)
        {
            DestroyProjectile();
        }
    }

    // Verifica se o projétil está dentro dos limites da tela
    bool IsWithinScreenBounds()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        return screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
    }

    // Movimento do projétil na direção fixa
    void MoveProjectile()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    // Detecta colisões com outros objetos
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Se colidir com o jogador
        {
            // Causa dano ao jogador
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); // Aplica o dano de 10
            }

            // Destroi o projétil após a colisão
            Destroy(gameObject);
        }
    }

    // Destroi o projétil manualmente
    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
