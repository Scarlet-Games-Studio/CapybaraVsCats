using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    public float speed = 0.3f; // Velocidade do movimento no eixo Y
    private float spriteHeight; // Altura do sprite para reposição
    private List<Transform> sprites; // Lista de todos os filhos (sprites)

    void Start()
    {
        // Calcula a altura do primeiro sprite
        spriteHeight = GetComponentInChildren<SpriteRenderer>().bounds.size.y;

        // Armazena todos os filhos (sprites) na lista
        sprites = new List<Transform>();
        foreach (Transform child in transform)
        {
            sprites.Add(child);
        }
    }

    void Update()
    {
        // Move todos os sprites para baixo no eixo Y
        foreach (Transform child in sprites)
        {
            child.position -= new Vector3(0, speed * Time.deltaTime, 0);
        }

        // Verifica se o último sprite saiu da tela e reposiciona os sprites
        if (sprites[0].position.y < -spriteHeight)
        {
            RepositionSprites();
        }
    }

    // Reposiciona os sprites para criar o efeito contínuo
    void RepositionSprites()
    {
        // Move o primeiro sprite para a posição do último
        Transform firstSprite = sprites[0];
        Transform lastSprite = sprites[sprites.Count - 1];

        firstSprite.position = lastSprite.position + new Vector3(0, spriteHeight, 0);

        // Remove o primeiro sprite da lista e coloca ele no final
        sprites.RemoveAt(0);
        sprites.Add(firstSprite);
    }
}
