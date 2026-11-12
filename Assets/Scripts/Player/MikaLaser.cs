using System.Collections.Generic;
using UnityEngine;

public class MikaLaser : MonoBehaviour
{
    [SerializeField] int damagePerTick = 10;
    [SerializeField] float tickInterval = 0.12f;
    [SerializeField] GameObject impactPrefab;
    [SerializeField, Min(0.1f)] float impactLifetime = 0.4f;
    [Header("Visual-synced damage window")]
    [SerializeField, Min(0)] int damageStartFrame = 24;
    [SerializeField, Min(0)] int damageEndFrame = 39;
    [SerializeField, Min(0.1f)] float damageWindowDuration = 2.8f;
    [SerializeField, Range(0.2f, 1f)] float beamHitboxWidth = 0.72f;
    readonly List<RaycastHit2D> beamHits = new();
    readonly HashSet<EnemyHealth> enemiesHitThisTick = new();
    SpriteSequencePlayer sequence;
    BoxCollider2D damageCollider;
    float damageWindowStartedAt;
    float nextDamageTick;
    bool damageWindowStarted;

    void Awake()
    {
        sequence = GetComponent<SpriteSequencePlayer>();
        damageCollider = GetComponent<BoxCollider2D>();
        if (damageCollider != null) damageCollider.enabled = false;
    }

    void LateUpdate()
    {
        if (damageCollider == null || sequence == null) return;
        int frame = sequence.CurrentFrame;
        bool insideFrameWindow = frame >= damageStartFrame && frame <= damageEndFrame;
        if (!insideFrameWindow && frame < damageStartFrame) damageWindowStarted = false;
        if (insideFrameWindow && !damageWindowStarted)
        {
            damageWindowStarted = true;
            damageWindowStartedAt = Time.time;
            nextDamageTick = Time.time;
        }

        bool shouldDamage = insideFrameWindow &&
            Time.time - damageWindowStartedAt < Mathf.Max(0.1f, damageWindowDuration);
        if (!shouldDamage)
        {
            enemiesHitThisTick.Clear();
            return;
        }

        if (Time.time < nextDamageTick) return;
        nextDamageTick = Time.time + Mathf.Max(0.05f, tickInterval);
        DamageEnemiesAlongVisibleBeam();
    }

    void DamageEnemiesAlongVisibleBeam()
    {
        if (damageCollider == null) return;

        Vector2 beamOrigin = transform.TransformPoint(
            damageCollider.offset + Vector2.up * (damageCollider.size.y * 0.5f));
        Vector2 beamDirection = -transform.up;
        float beamLength = damageCollider.size.y * Mathf.Abs(transform.lossyScale.y);
        float beamWidth = damageCollider.size.x * Mathf.Abs(transform.lossyScale.x) * beamHitboxWidth;
        float angle = transform.eulerAngles.z;

        beamHits.Clear();
        enemiesHitThisTick.Clear();
        Physics2D.BoxCast(
            beamOrigin,
            new Vector2(Mathf.Max(0.05f, beamWidth), 0.02f),
            angle,
            beamDirection,
            new ContactFilter2D { useTriggers = true },
            beamHits,
            beamLength);

        foreach (RaycastHit2D hit in beamHits)
        {
            EnemyHealth enemy = hit.collider != null ? hit.collider.GetComponentInParent<EnemyHealth>() : null;
            if (enemy == null || enemy.IsDead || !enemiesHitThisTick.Add(enemy)) continue;

            Vector3 impactPosition = hit.point;
            if (impactPosition == Vector3.zero) impactPosition = enemy.transform.position;
            SpawnImpact(impactPosition);
            enemy.TakeDamage(damagePerTick);
        }
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

}
