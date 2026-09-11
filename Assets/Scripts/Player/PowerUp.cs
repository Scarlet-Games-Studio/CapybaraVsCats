using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Health, FireRate, Speed, DoubleShot }

    public PowerUpType powerUpType;
    [Min(1)] public int value = 20;
    [SerializeField, Min(0f)] float fallSpeed = 1.25f;
    [SerializeField, Min(0.1f)] float lifetime = 12f;

    bool collected;

    void Awake()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;
    }

    void Start() => Destroy(gameObject, lifetime);

    void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);
        transform.Rotate(0f, 0f, 30f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || !player.CompareTag("Player")) return;
        if (!ApplyPowerUp(player)) return;

        collected = true;
        Destroy(gameObject);
    }

    bool ApplyPowerUp(PlayerController player)
    {
        switch (powerUpType)
        {
            case PowerUpType.Health:
                Health health = player.GetComponent<Health>();
                if (health == null || health.currentHealth >= health.maxHealth) return false;
                health.Heal(value);
                return true;

            case PowerUpType.FireRate:
                player.IncreaseFireRatePercent(Mathf.Clamp(value, 1, 80) / 100f);
                return true;

            case PowerUpType.Speed:
                player.IncreaseSpeedPercent(Mathf.Clamp(value, 1, 100) / 100f);
                return true;

            case PowerUpType.DoubleShot:
                return player.PowerUpBullet();

            default:
                return false;
        }
    }
}
