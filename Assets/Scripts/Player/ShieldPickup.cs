using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShieldPickup : MonoBehaviour
{
    [SerializeField] float fallSpeed = 1.5f;
    [SerializeField] float lifetime = 12f;
    [SerializeField] int shieldHits = 3;
    [SerializeField] GameObject hiroShield;
    [SerializeField] GameObject mikaShield;
    [SerializeField] GameObject edgeShield;

    void Start() => Destroy(gameObject, lifetime);

    void Update()
    {
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);
        transform.Rotate(0f, 0f, 45f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || !player.CompareTag("Player")) return;

        ShieldController controller = player.GetComponent<ShieldController>();
        if (controller == null) controller = player.gameObject.AddComponent<ShieldController>();
        controller.Activate(SelectShield(player.gameObject), shieldHits);
        Destroy(gameObject);
    }

    GameObject SelectShield(GameObject player)
    {
        string id = player.name.ToLowerInvariant();
        if (CharacterSelection.Selected == CharacterSelection.Character.Mika ||
            id.Contains("mika") || id.Contains("garota")) return mikaShield;
        if (CharacterSelection.Selected == CharacterSelection.Character.Edge ||
            id.Contains("edge") || id.Contains("jack")) return edgeShield;
        return hiroShield;
    }
}
