using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeath : MonoBehaviour
{
    [SerializeField, Min(0)] int scoreValue = 100;

    EnemyHealth health;
    bool rewarded;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        health.Died += OnDefeated;
    }

    void OnDestroy()
    {
        if (health != null) health.Died -= OnDefeated;
    }

    void OnDefeated()
    {
        // Pontos só são concedidos por uma morte real, nunca por unload/despawn.
        if (rewarded || CompareTag("Boss")) return;
        rewarded = true;
        ScoreManager.AddScore(scoreValue);
    }
}
