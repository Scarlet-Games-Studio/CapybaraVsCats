using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    // Velocidade do movimento no eixo Y
    public float velocidadeY = 0.1f;

    // Referência para o componente RawImage
    private RawImage rawImage;

    // Posição inicial da textura
    private Vector2 offset;

    void Start()
    {
        // Obtém o componente RawImage anexado ao objeto
        rawImage = GetComponent<RawImage>();

        // Inicializa o offset
        offset = rawImage.uvRect.position;
    }

    void Update()
    {
        // Calcula o novo offset baseado na velocidade e no tempo
        offset.y += velocidadeY * Time.deltaTime;

        // Atualiza a posição da textura na RawImage
        rawImage.uvRect = new Rect(offset, rawImage.uvRect.size);

        // Verifica se o offset atingiu 1 ou -1 para fazer o loop
        if (offset.y > 1f)
        {
            offset.y -= 1f;
        }
        else if (offset.y < -1f)
        {
            offset.y += 1f;
        }
    }
}
