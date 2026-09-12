using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1900)]
public class PowerUpDropSystem : MonoBehaviour
{
    const string GameplayScene = "ingame";
    static PowerUpDropSystem instance;

    PowerUpDropCatalog catalog;
    int killsWithoutDrop;
    float nextAllowedDropTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureCreated()
    {
        if (instance != null) return;
        GameObject root = new("Power Up Drop System");
        instance = root.AddComponent<PowerUpDropSystem>();
        DontDestroyOnLoad(root);
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        catalog = Resources.Load<PowerUpDropCatalog>("PowerUpDropCatalog");
    }

    void OnEnable()
    {
        EnemyHealth.EnemyDefeated += OnEnemyDefeated;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        EnemyHealth.EnemyDefeated -= OnEnemyDefeated;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GameplayScene) return;
        killsWithoutDrop = 0;
        nextAllowedDropTime = 0f;
    }

    void OnEnemyDefeated(EnemyHealth enemy)
    {
        if (enemy == null || SceneManager.GetActiveScene().name != GameplayScene) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;
        if (enemy.GetComponent<GatoballDrop>() != null) return;
        if (catalog == null) catalog = Resources.Load<PowerUpDropCatalog>("PowerUpDropCatalog");
        if (catalog == null) return;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        List<DropKind> eligible = GetEligibleDrops(player);
        if (eligible.Count == 0) return;

        killsWithoutDrop++;
        bool pity = killsWithoutDrop >= Mathf.Max(1, catalog.pityKills);
        if (Time.time < nextAllowedDropTime || (!pity && Random.value > Mathf.Clamp01(catalog.dropChance))) return;

        DropKind choice = eligible[Random.Range(0, eligible.Count)];
        if (Spawn(choice, enemy.transform.position))
        {
            killsWithoutDrop = 0;
            nextAllowedDropTime = Time.time + Mathf.Max(0f, catalog.minimumDropInterval);
        }
    }

    List<DropKind> GetEligibleDrops(PlayerController player)
    {
        List<DropKind> result = new(2);
        if (player.FireTier == 1 && catalog.doubleShotSprite != null) result.Add(DropKind.DoubleShot);
        else if (player.FireTier == 2 && catalog.tripleShotSprite != null) result.Add(DropKind.TripleShot);

        ShieldController shield = player.GetComponent<ShieldController>();
        if ((shield == null || !shield.IsActive) && catalog.shieldDropPrefab != null) result.Add(DropKind.Shield);
        return result;
    }

    bool Spawn(DropKind kind, Vector3 position)
    {
        if (kind == DropKind.Shield)
        {
            GameObject shield = Instantiate(catalog.shieldDropPrefab, position, Quaternion.identity);
            shield.transform.localScale = Vector3.one;
            PowerUpDropMotion motion = shield.GetComponent<PowerUpDropMotion>();
            if (motion == null) motion = shield.AddComponent<PowerUpDropMotion>();
            motion.ConfigureVisual(catalog.shieldSprite);
            return true;
        }

        Sprite sprite = kind == DropKind.DoubleShot ? catalog.doubleShotSprite : catalog.tripleShotSprite;
        if (sprite == null) return false;

        string label = kind == DropKind.DoubleShot ? "Double Fire Power Up" : "Triple Fire Power Up";
        GameObject drop = new(label, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D), typeof(PowerUp), typeof(PowerUpDropMotion));
        drop.transform.position = position;

        CircleCollider2D collider = drop.GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
        Rigidbody2D body = drop.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        PowerUp pickup = drop.GetComponent<PowerUp>();
        pickup.Configure(kind == DropKind.DoubleShot ? PowerUp.PowerUpType.DoubleShot : PowerUp.PowerUpType.TripleShot);
        drop.GetComponent<PowerUpDropMotion>().ConfigureVisual(sprite);
        return true;
    }

    enum DropKind { DoubleShot, TripleShot, Shield }
}
