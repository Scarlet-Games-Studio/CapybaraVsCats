using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    // Velocidade do movimento no eixo Y
    public float speed = 0.1f;

    // Altura do sprite para reiniciar a posição
    private float spriteHeight;

    // Personagem
    public GameObject boss;

    // Variável para verificar se o boss está vivo
    public bool isBossAlive = true;

    void Start()
    {
        // Calcula a altura do sprite
        spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        if(boss == null)
        {
            EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();

            if (enemySpawner != null)
            {
                enemySpawner.canSpawn = true;
            }
        }
        else
        {
            if (IsWithinDistance(boss, 1f))
            {
                EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();

                if (enemySpawner != null)
                {
                    enemySpawner.canSpawn = false;
                }
            }

            // Verifica se o boss está completamente visível e se ele está vivo
            if (IsFullyVisible(boss))
            {
                BossScript bossScript = boss.GetComponent<BossScript>();
                bossScript.canShoot = true;

                // Se o boss estiver completamente visível e vivo, para o scroll
                return;
            }
        }

        // Move o sprite no eixo Y
        transform.position -= new Vector3(0, speed * Time.deltaTime, 0);

        // Reinicia a posição do sprite quando ele sai da tela
        // if (transform.position.y < -spriteHeight)
        // {
        //     transform.position += new Vector3(0, 2 * spriteHeight, 0);
        // }
    }


    bool IsFullyVisible(GameObject obj)
    {
        Renderer objectRenderer = obj.GetComponent<Renderer>();
        Vector3[] objectCorners = new Vector3[2];
        objectCorners[0] = objectRenderer.bounds.min;
        objectCorners[1] = objectRenderer.bounds.max;

        foreach (Vector3 corner in objectCorners)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(corner);
            if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            {
                return false;
            }
        }

        return true;
    }


    bool IsWithinDistance(GameObject obj, float distance)
    {
        Renderer objectRenderer = obj.GetComponent<Renderer>();
        Vector3[] objectCorners = new Vector3[2];
        objectCorners[0] = objectRenderer.bounds.min;
        objectCorners[1] = objectRenderer.bounds.max;

        foreach (Vector3 corner in objectCorners)
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(corner);
            if (viewportPos.x < -distance || viewportPos.x > 1 + distance || viewportPos.y < -distance || viewportPos.y > 1 + distance)
            {
                return false;
            }
        }

        return true;
    }
}
