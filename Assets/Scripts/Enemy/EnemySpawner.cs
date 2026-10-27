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

    [Header("Stage progression")]
    [Tooltip("Referência ao GameObject skylauch4_0 (7). O boss entra quando o pivô do painel chega ao centro da tela.")]
    [SerializeField] Transform bossTriggerSky;
    [SerializeField, Range(0.05f, 0.95f)] float bossTriggerViewportY = 0.5f;
    [SerializeField, Range(0.02f, 0.1f)] float waveProgressStep = 0.045f;

    [Header("Mixed kamikaze spawn")]
    [SerializeField] GameObject kamikazePrefab;
    [SerializeField, Range(0f, 1f)] float kamikazeChance = 0.3f;
    [SerializeField, Min(0f)] float kamikazeHorizontalOffset = 0.7f;
    [SerializeField, Min(0.1f)] float minimumSpawnSeparation = 0.35f;

    [Header("GatoCobra encounters")]
    [SerializeField] GameObject gatoCobraPrefab;
    [SerializeField, Range(0, 2)] int gatoCobraEncounters = 2;

    Coroutine spawnRoutine;
    Coroutine bossRoutine;
    float initialBossSkyViewportY;
    bool canTrackSkyProgress;
    ParallaxScroller arenaParallax;

    void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        arenaParallax = FindAnyObjectByType<ParallaxScroller>();
        if (targetCamera == null)
        {
            Debug.LogError("EnemySpawner não encontrou uma câmera de gameplay.", this);
            enabled = false;
            return;
        }

        AddGatoCobraEncounters();
        if (bossTriggerSky != null)
        {
            initialBossSkyViewportY = GetBossTriggerViewportY();
            canTrackSkyProgress = initialBossSkyViewportY > bossTriggerViewportY + 0.01f;
            if (!canTrackSkyProgress)
                Debug.LogWarning("EnemySpawner: skylauch11 já está abaixo do gatilho; usando o ritmo de ondas configurado.", this);
        }
        spawnRoutine = StartCoroutine(SpawnWavesWithDelay());
        EnemyWave bossWave = waves != null
            ? System.Array.Find(waves, wave => wave != null && wave.boss && wave.enemyPrefab != null)
            : null;
        if (canTrackSkyProgress && bossWave != null)
            bossRoutine = StartCoroutine(SpawnBossOnSkyArrival(bossWave));
    }

    void AddGatoCobraEncounters()
    {
        if (gatoCobraEncounters <= 0 || waves == null) return;
        if (gatoCobraPrefab == null) gatoCobraPrefab = Resources.Load<GameObject>("Enemies/GatoCobra");
        if (gatoCobraPrefab == null)
        {
            Debug.LogWarning("Prefab do GatoCobra nao foi encontrado em Resources/Enemies.", this);
            return;
        }

        var expanded = new System.Collections.Generic.List<EnemyWave>(waves);
        if (expanded.Exists(wave => wave != null && wave.enemyPrefab == gatoCobraPrefab)) return;

        int regularWaveCount = expanded.FindAll(wave => wave != null && !wave.boss).Count;
        int added = Mathf.Min(gatoCobraEncounters, 2);
        for (int encounter = 0; encounter < added; encounter++)
        {
            int insertion = Mathf.Clamp((regularWaveCount * (encounter + 1)) / (added + 1), 0, expanded.Count);
            int bossIndex = expanded.FindIndex(wave => wave != null && wave.boss);
            if (bossIndex >= 0) insertion = Mathf.Min(insertion, bossIndex);
            expanded.Insert(insertion, CreateGatoCobraWave());
        }

        waves = expanded.ToArray();
    }

    EnemyWave CreateGatoCobraWave() => new EnemyWave
    {
        enemyPrefab = gatoCobraPrefab,
        enemyCount = 1,
        spawnInterval = 0f,
        boss = false
    };

    void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
        if (bossRoutine != null)
        {
            StopCoroutine(bossRoutine);
            bossRoutine = null;
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

        if (!CanContinue() || waves == null) yield break;
        var regularWaves = new System.Collections.Generic.List<EnemyWave>();
        foreach (EnemyWave wave in waves)
        {
            if (wave != null && !wave.boss && wave.enemyPrefab != null && wave.enemyCount > 0)
                regularWaves.Add(wave);
        }

        if (canTrackSkyProgress && bossTriggerSky != null)
        {
            // Começa a cadência depois da espera inicial e recicla os grupos até o boss chegar.
            initialBossSkyViewportY = GetBossTriggerViewportY();
            float step = Mathf.Max(0.02f, waveProgressStep);
            int wavesUntilBoss = Mathf.Max(1, Mathf.FloorToInt(0.96f / step));

            for (int i = 0; i < wavesUntilBoss && CanContinue() && !IsBossTriggerReached(); i++)
            {
                if (regularWaves.Count == 0) break;

                float progress = Mathf.Min(0.96f, step * (i + 1));
                yield return WaitForSkyProgress(progress);
                if (!CanContinue() || IsBossTriggerReached()) break;

                yield return SpawnEnemiesInWave(regularWaves[i % regularWaves.Count]);
            }

            if (regularWaves.Count > 0 && !IsBossTriggerReached())
            {
                yield return WaitForSkyProgress(0.98f);
                if (CanContinue() && !IsBossTriggerReached())
                    yield return SpawnEnemiesInWave(regularWaves[wavesUntilBoss % regularWaves.Count]);
            }
        }
        else
        {
            foreach (EnemyWave wave in regularWaves)
            {
                if (!CanContinue()) yield break;
                yield return SpawnEnemiesInWave(wave);
                yield return new WaitForSeconds(timeBetweenWaves);
            }

            EnemyWave bossWave = System.Array.Find(waves, wave => wave != null && wave.boss && wave.enemyPrefab != null);
            if (bossWave != null && CanContinue()) yield return SpawnEnemiesInWave(bossWave);
        }

        spawnRoutine = null;
    }

    IEnumerator SpawnBossOnSkyArrival(EnemyWave bossWave)
    {
        while (CanContinue() && !IsBossTriggerReached()) yield return null;
        if (CanContinue() && IsBossTriggerReached()) yield return SpawnEnemiesInWave(bossWave);
        bossRoutine = null;
    }

    bool IsBossTriggerReached()
    {
        if (!canTrackSkyProgress || bossTriggerSky == null || targetCamera == null) return false;
        return GetBossTriggerViewportY() <= bossTriggerViewportY;
    }

    IEnumerator WaitForSkyProgress(float progress)
    {
        float triggerY = bossTriggerViewportY;
        float targetY = Mathf.Lerp(initialBossSkyViewportY, triggerY, Mathf.Clamp01(progress));
        while (CanContinue() && bossTriggerSky != null)
        {
            float currentY = GetBossTriggerViewportY();
            if (currentY <= targetY) yield break;
            yield return null;
        }
    }

    float GetBossTriggerViewportY()
    {
        return bossTriggerSky != null
            ? targetCamera.WorldToViewportPoint(bossTriggerSky.position).y
            : float.NegativeInfinity;
    }

    IEnumerator SpawnEnemiesInWave(EnemyWave wave)
    {
        int count = wave.boss ? 1 : wave.enemyCount;
        for (int i = 0; i < count; i++)
        {
            if (!CanContinue()) yield break;
            if (!wave.boss && bossSpawned) yield break;

            if (wave.boss)
            {
                if (bossSpawned) yield break;
                bossSpawned = true;
                if (arenaParallax != null) arenaParallax.StopForBossArena();
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

    bool bossSpawned;

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
