using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 2f;
    public bool canSpawn = true;

    void Start()
    {
        InvokeRepeating("TrySpawnEnemy", 2f, spawnRate);
    }

    void TrySpawnEnemy()
    {
        if (canSpawn)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        float screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x + 0.1f;
        float screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - 0.1f;

        float spawnX = Random.Range(screenLeft, screenRight);
        Vector2 spawnPosition = new Vector2(spawnX, 6f); // Ajuste conforme necessário
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}



