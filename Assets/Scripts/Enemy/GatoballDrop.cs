using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class GatoballDrop : MonoBehaviour
{
    [SerializeField] GameObject shieldDropPrefab;
    EnemyHealth health;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        health.Died += DropShield;
    }

    void OnDestroy()
    {
        if (health != null) health.Died -= DropShield;
    }

    void DropShield()
    {
        if (shieldDropPrefab != null)
            Instantiate(shieldDropPrefab, transform.position, Quaternion.identity);
    }
}
