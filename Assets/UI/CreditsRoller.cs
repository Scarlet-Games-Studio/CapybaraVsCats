using System.Collections;
using UnityEngine;

public class CreditsRoller : MonoBehaviour
{
    public RectTransform creditsText; // O componente RectTransform do texto dos créditos
    public float scrollSpeed = 50f; // Velocidade de rolagem do texto
    public float startDelay = 2f; // Tempo de espera antes de começar a rolar
    public float endDelay = 2f; // Tempo de espera ao terminar
    public bool loop = false; // Determina se os créditos reiniciam após terminar

    private float initialPosition; // Posição inicial do texto
    private float endPosition; // Posição final do texto
    private Coroutine rollCoroutine; // Referência ao coroutine ativo

    void Start()
    {
        // Calcula a posição inicial e final do texto
        initialPosition = creditsText.anchoredPosition.y;
        endPosition = -creditsText.rect.height; // Altura negativa do texto para ir além da área visível

        // Inicia a rolagem dos créditos
        rollCoroutine = StartCoroutine(RollCredits());
    }

    IEnumerator RollCredits()
    {
        yield return new WaitForSeconds(startDelay); // Espera antes de começar

        while (true)
        {
            // Move o texto verticalmente
            while (creditsText.anchoredPosition.y > endPosition)
            {
                creditsText.anchoredPosition += new Vector2(0, -scrollSpeed * Time.deltaTime);
                yield return null;
            }

            // Espera ao terminar
            yield return new WaitForSeconds(endDelay);

            if (loop)
            {
                // Reinicia a posição do texto
                creditsText.anchoredPosition = new Vector2(creditsText.anchoredPosition.x, initialPosition);
            }
            else
            {
                break; // Sai do loop se não for para reiniciar
            }
        }
    }

    // Método para reiniciar o movimento dos créditos
    public void RestartCredits()
    {
        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine); // Para o coroutine atual
        }

        // Reseta a posição inicial do texto
        creditsText.anchoredPosition = new Vector2(creditsText.anchoredPosition.x, initialPosition);

        // Reinicia o coroutine
        rollCoroutine = StartCoroutine(RollCredits());
    }
}
