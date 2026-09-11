using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public static event System.Action<EnemyHealth> EnemyDefeated;
    public int health = 20; // Vida inicial do inimigo
    public bool IsDead { get; private set; }
    public event System.Action Died;

    // Método para aplicar dano ao inimigo
    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return;

        // O boss também possui EnemyHealth para ser detectado por lasers/áreas.
        // Centraliza o dano no controlador real para ele não sumir aos 20 HP.
        BossG1BulletPattern boss = GetComponent<BossG1BulletPattern>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            return;
        }

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
        if (IsDead) return;
        IsDead = true;
        Died?.Invoke();
        EnemyDefeated?.Invoke(this);
        // Você pode adicionar aqui o código para destruir o inimigo ou ativar a animação de morte
        Destroy(gameObject); // Destrói o inimigo
    }
}
