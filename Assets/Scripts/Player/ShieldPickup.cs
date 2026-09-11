using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldPickup : MonoBehaviour
{
    [SerializeField, Min(0f)] float fallSpeed = 1.5f;
    [SerializeField, Min(0.1f)] float lifetime = 12f;
    [SerializeField, Min(1)] int shieldHits = 3;
    [SerializeField] GameObject hiroShield;
    [SerializeField] GameObject mikaShield;
    [SerializeField] GameObject edgeShield;

    bool collected;

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

    void Start() => Destroy(gameObject, lifetime);

    void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);
        transform.Rotate(0f, 0f, 45f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || !player.CompareTag("Player")) return;

        GameObject shieldPrefab = SelectShield(player.gameObject);
        if (shieldPrefab == null)
        {
            Debug.LogWarning($"Escudo não configurado para {player.name}; pickup preservado.", this);
            return;
        }

        collected = true;
        ShieldController controller = player.GetComponent<ShieldController>();
        if (controller == null) controller = player.gameObject.AddComponent<ShieldController>();
        controller.Activate(shieldPrefab, shieldHits);
        Destroy(gameObject);
    }

    GameObject SelectShield(GameObject player)
    {
        string id = player.name.ToLowerInvariant();
        if (CharacterSelection.Selected == CharacterSelection.Character.Mika || id.Contains("mika") || id.Contains("garota"))
            return mikaShield != null ? mikaShield : hiroShield;
        if (CharacterSelection.Selected == CharacterSelection.Character.Edge || id.Contains("edge") || id.Contains("jack"))
            return edgeShield != null ? edgeShield : hiroShield;
        return hiroShield;
    }
}
