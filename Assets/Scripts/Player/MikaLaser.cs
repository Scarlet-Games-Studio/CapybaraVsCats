using System.Collections.Generic;
using UnityEngine;

public class MikaLaser : MonoBehaviour
{
    [SerializeField] int damagePerTick = 10;
    [SerializeField] float tickInterval = 0.12f;
    [SerializeField] GameObject impactPrefab;
    [SerializeField, Min(0.1f)] float impactLifetime = 0.4f;
    [Header("Dynamic damage window")]
    [SerializeField, Min(0)] int damageStartFrame = 18;
    [SerializeField, Min(0)] int damageEndFrame = 39;
    readonly Dictionary<EnemyHealth, float> nextDamage = new();
    SpriteSequencePlayer sequence;
    Collider2D damageCollider;

    void Awake()
    {
        sequence = GetComponent<SpriteSequencePlayer>();
        damageCollider = GetComponent<Collider2D>();
        if (damageCollider != null) damageCollider.enabled = false;
    }

    void LateUpdate()
    {
        if (damageCollider == null || sequence == null) return;
        int frame = sequence.CurrentFrame;
        bool shouldDamage = frame >= damageStartFrame && frame <= damageEndFrame;
        if (damageCollider.enabled != shouldDamage)
        {
            damageCollider.enabled = shouldDamage;
            if (!shouldDamage) nextDamage.Clear();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (damageCollider == null || !damageCollider.enabled) return;
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null || enemy.IsDead) return;
        if (nextDamage.TryGetValue(enemy, out float next) && Time.time < next) return;
        nextDamage[enemy] = Time.time + tickInterval;
        SpawnImpact(other.bounds.center);
        enemy.TakeDamage(damagePerTick);
    }

    void SpawnImpact(Vector3 position)
    {
        if (impactPrefab == null) return;
        GameObject impact = Instantiate(
            impactPrefab,
            position,
            Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f)));
        Destroy(impact, impactLifetime);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null) nextDamage.Remove(enemy);
    }
}
