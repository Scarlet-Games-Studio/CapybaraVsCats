using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Health, FireRate, Speed, DoubleShot, TripleShot }

    public PowerUpType powerUpType;
    [Min(1)] public int value = 20;
    [SerializeField, Min(0f)] float fallSpeed = 1.25f;
    [SerializeField, Min(0.1f)] float lifetime = 12f;

    bool collected;

    public void Configure(PowerUpType type, float speed = 1.25f, float duration = 12f)
    {
        powerUpType = type;
        fallSpeed = Mathf.Max(0.1f, speed);
        lifetime = Mathf.Max(1f, duration);
    }

    void Awake()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Start()
    {
        PowerUpDropMotion motion = GetComponent<PowerUpDropMotion>();
        if (motion == null) motion = gameObject.AddComponent<PowerUpDropMotion>();
        motion.Configure(fallSpeed, lifetime);
        Destroy(gameObject, lifetime);
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
                return player.UpgradeFireMode(2);

            case PowerUpType.TripleShot:
                return player.UpgradeFireMode(3);

            default:
                return false;
        }
    }
}
