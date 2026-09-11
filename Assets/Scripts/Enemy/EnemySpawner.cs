using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWave
    {
        public GameObject enemyPrefab;
        [Min(0)] public int enemyCount;
        [Min(0f)] public float spawnInterval;
        public bool boss;
    }

    public Camera targetCamera;
    public EnemyWave[] waves;
    [Min(0f)] public float timeBetweenWaves = 3f;
    public float spawnOffsetY = 1f;
    [Min(0f)] public float spawnDelay = 10f;
    public bool canSpawn = true;
    public Transform bossSpawn;

    [Header("Mixed kamikaze spawn")]
    [SerializeField] GameObject kamikazePrefab;
    [SerializeField, Range(0f, 1f)] float kamikazeChance = 0.3f;
    [SerializeField, Min(0f)] float kamikazeHorizontalOffset = 0.7f;
    [SerializeField, Min(0.1f)] float minimumSpawnSeparation = 0.35f;

    Coroutine spawnRoutine;

    void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogError("EnemySpawner não encontrou uma câmera de gameplay.", this);
            enabled = false;
            return;
        }

        spawnRoutine = StartCoroutine(SpawnWavesWithDelay());
    }

    void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void StopSpawning()
    {
        canSpawn = false;
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    IEnumerator SpawnWavesWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (waves == null) yield break;
        foreach (EnemyWave wave in waves)
        {
            if (!CanContinue()) yield break;
            if (wave == null || wave.enemyPrefab == null || wave.enemyCount <= 0)
            {
                Debug.LogWarning("Uma onda inválida foi ignorada.", this);
                continue;
            }

            yield return SpawnEnemiesInWave(wave);
            if (!CanContinue()) yield break;
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        spawnRoutine = null;
    }

    IEnumerator SpawnEnemiesInWave(EnemyWave wave)
    {
        int count = wave.boss ? 1 : wave.enemyCount;
        for (int i = 0; i < count; i++)
        {
            if (!CanContinue()) yield break;

            if (wave.boss)
            {
                Vector3 position = bossSpawn != null ? bossSpawn.position : ViewportToWorld(0.5f, 0.82f);
                Instantiate(wave.enemyPrefab, position, Quaternion.identity);
            }
            else
            {
                Vector2 spawnPosition = GetSpawnPosition();
                Instantiate(wave.enemyPrefab, spawnPosition, Quaternion.identity);

                if (kamikazePrefab != null && wave.enemyPrefab != kamikazePrefab && Random.value <= kamikazeChance)
                {
                    float side = Random.value < 0.5f ? -1f : 1f;
                    Vector2 bounds = GetHorizontalBounds();
                    float kamikazeX = Mathf.Clamp(
                        spawnPosition.x + side * Mathf.Max(kamikazeHorizontalOffset, minimumSpawnSeparation),
                        bounds.x,
                        bounds.y);
                    Instantiate(kamikazePrefab, new Vector2(kamikazeX, spawnPosition.y), Quaternion.identity);
                }
            }

            if (i < count - 1) yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    bool CanContinue()
    {
        return canSpawn && (GameManager.instance == null || GameManager.instance.IsPlaying);
    }

    Vector2 GetSpawnPosition()
    {
        Vector2 bounds = GetHorizontalBounds();
        float x = Random.Range(bounds.x, bounds.y);
        Vector3 top = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, CameraDepth()));
        return new Vector2(x, top.y + spawnOffsetY);
    }

    Vector2 GetHorizontalBounds()
    {
        float depth = CameraDepth();
        float left = targetCamera.ViewportToWorldPoint(new Vector3(0.06f, 0.5f, depth)).x;
        float right = targetCamera.ViewportToWorldPoint(new Vector3(0.94f, 0.5f, depth)).x;
        return new Vector2(Mathf.Min(left, right), Mathf.Max(left, right));
    }

    Vector3 ViewportToWorld(float x, float y)
    {
        return targetCamera.ViewportToWorldPoint(new Vector3(x, y, CameraDepth()));
    }

    float CameraDepth()
    {
        return Mathf.Abs(transform.position.z - targetCamera.transform.position.z);
    }
}
