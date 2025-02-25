using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private Vector2 direction; // Dire��o fixa do proj�til
    public float speed = 10f; // Velocidade do proj�til
    public int damage = 10; // Dano causado pelo proj�til
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

            // Destroi o proj�til ap�s a colis�o
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
