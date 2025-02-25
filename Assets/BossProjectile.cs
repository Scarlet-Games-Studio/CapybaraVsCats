using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private Vector2 direction; // Direção fixa do projétil
    public float speed = 10f; // Velocidade do projétil
    public int damage = 10; // Dano causado pelo projétil
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void MoveProjectile()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
