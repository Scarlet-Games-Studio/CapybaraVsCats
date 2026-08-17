using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        ShieldController shield = GetComponent<ShieldController>();
        if (shield != null && shield.TryAbsorbHit())
            return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    void Die()
    {
        if (gameObject.CompareTag("Player"))
        {
            if (GameManager.instance != null)
                GameManager.instance.GameOver();
            else
                Debug.LogWarning("GameManager não foi encontrado ao processar Game Over.");
        }
        Destroy(gameObject);
    }
}
