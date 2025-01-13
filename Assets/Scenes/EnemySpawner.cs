using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWave
    {
        public GameObject enemyPrefab;  // Prefab do inimigo
        public int enemyCount;         // Quantidade de inimigos nesta onda
        public float spawnInterval;    // Tempo entre os spawns dentro da onda
    }

    public Camera targetCamera;          // Câmera vinculada manualmente no Inspector
    public EnemyWave[] waves;            // Lista de ondas
    public float timeBetweenWaves = 3f;  // Tempo entre cada onda
    public float spawnOffsetY = 1f;      // Ajuste da posição inicial de spawn (acima da tela)
    public float spawnDelay = 10f;       // Tempo antes do início do spawn (ajustável no Inspector)
    public bool canSpawn = true;         // Controle para ativar/desativar o spawn

    private float screenLeft, screenRight, screenTop; // Limites da câmera

    void Start()
    {
        if (targetCamera == null)
        {
            Debug.LogError("Nenhuma câmera vinculada! Por favor, arraste uma câmera no campo 'Target Camera' no Inspector.");
            return;
        }

        // Calcula os limites da câmera
        Vector3 screenBottomLeft = targetCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenTopRight = targetCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        screenLeft = screenBottomLeft.x;
        screenRight = screenTopRight.x;
        screenTop = screenTopRight.y;

        StartCoroutine(SpawnWavesWithDelay());
    }

    IEnumerator SpawnWavesWithDelay()
    {
        // Espera o tempo configurado antes de começar o spawn
        yield return new WaitForSeconds(spawnDelay);

        // Inicia o spawn das ondas
        foreach (var wave in waves)
        {
            if (!canSpawn) yield break; // Para o spawn se `canSpawn` for falso
            yield return StartCoroutine(SpawnEnemiesInWave(wave));
            yield return new WaitForSeconds(timeBetweenWaves); // Espera entre ondas
        }
    }

    IEnumerator SpawnEnemiesInWave(EnemyWave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            if (!canSpawn) yield break; // Interrompe o spawn se necessário

            // Calcula uma posição aleatória acima da tela, com ajuste do offset vertical
            float spawnX = Random.Range(screenLeft, screenRight);
            float spawnY = screenTop + spawnOffsetY; // Usa o offset configurável
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);

            // Instancia o inimigo
            Instantiate(wave.enemyPrefab, spawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(wave.spawnInterval); // Intervalo entre inimigos
        }
    }
}
