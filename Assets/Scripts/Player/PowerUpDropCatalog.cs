using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpDropCatalog", menuName = "Capybara Vs Cats/Power Up Drop Catalog")]
public class PowerUpDropCatalog : ScriptableObject
{
    [Header("Balance")]
    [Range(0f, 1f)] public float dropChance = 0.24f;
    [Min(1)] public int pityKills = 6;
    [Min(0f)] public float minimumDropInterval = 2.5f;

    [Header("Visuals")]
    public Sprite doubleShotSprite;
    public Sprite tripleShotSprite;
    public Sprite shieldSprite;
    public GameObject shieldDropPrefab;
}
