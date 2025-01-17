using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 20; // Vida inicial do inimigo

    // Método para aplicar dano ao inimigo
    public void TakeDamage(int damage)
    {
        health -= damage;

        // Se a vida chegar a zero ou menos, o inimigo morre
        if (health <= 0)
        {
            Die();
        }
    }

    // Método chamado quando o inimigo morre
    void Die()
    {
        Debug.Log("Inimigo morreu!");
        // Você pode adicionar aqui o código para destruir o inimigo ou ativar a animação de morte
        Destroy(gameObject); // Destrói o inimigo
    }
}
