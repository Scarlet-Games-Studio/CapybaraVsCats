using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f; // Velocidade do projétil
    public float lifeTime = 2f; // Duração do projétil antes de ser destruído
    public int damage = 10; // Dano causado pelo projétil
    public float homingStrength = 0.00001f; // Força de homing extremamente leve
    private Vector2 direction; // Direção atual do projétil

    void Start()
    {
        // Inicializa a direção do projétil
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            direction = (playerObject.transform.position - transform.position).normalized; // Direção inicial para o jogador
        }
        else
        {
            direction = Vector2.down; // Se o jogador não for encontrado, o projétil seguirá para baixo
        }

        // Destroi o projétil automaticamente após 'lifeTime' segundos
        Invoke("DestroyProjectile", lifeTime);
    }

    void Update()
    {
        // Se o jogador estiver presente, ajusta levemente a direção
        if (direction != Vector2.zero)
        {
            AdjustDirectionTowardsPlayer();
        }

        // Move o projétil de acordo com a direção calculada
        MoveProjectile();

        // Destroi o projétil se sair da tela
        if (!IsWithinScreenBounds())
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

    // Ajusta a direção do projétil levemente em direção ao jogador
    void AdjustDirectionTowardsPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            Vector2 directionToPlayer = (playerObject.transform.position - transform.position).normalized; // Direção para o jogador

            // Suaviza o ajuste da direção, de modo que o projétil ainda siga reto, mas levemente em direção ao jogador
            direction = Vector2.Lerp(direction, directionToPlayer, homingStrength); // Lerp é uma interpolação linear suave
        }
    }

    // Movimento do projétil na direção calculada
    void MoveProjectile()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World); // Move com direção ajustada
    }

    // Detecta colisões com outros objetos
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Se colidir com o jogador
        {
            // Causa dano ao jogador (dano definido como 10)
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
