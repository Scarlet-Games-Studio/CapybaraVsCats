using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Min(1)] public int maxHealth = 100;
    public int currentHealth;
    [SerializeField, Min(0f)] float reviveInvulnerability = 2f;

    public bool IsDead { get; private set; }
    public event Action<int, int> Changed;

    float invulnerableUntil;

    void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0 || Time.unscaledTime < invulnerableUntil) return;

        ShieldController shield = GetComponent<ShieldController>();
        if (shield != null && shield.TryAbsorbHit()) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Changed?.Invoke(currentHealth, maxHealth);
        if (currentHealth == 0) Die();
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Changed?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (CompareTag("Player"))
        {
            FMODManager.PlayPlayerDeath(transform.position);
            if (GameManager.instance != null)
                GameManager.instance.GameOver();
            else
                Debug.LogWarning("GameManager não foi encontrado ao processar Game Over.");
            return;
        }

        Destroy(gameObject);
    }

    public bool RevivePlayer()
    {
        if (!CompareTag("Player")) return false;
        IsDead = false;
        currentHealth = maxHealth;
        invulnerableUntil = Time.unscaledTime + reviveInvulnerability;
        Changed?.Invoke(currentHealth, maxHealth);
        return true;
    }
}
