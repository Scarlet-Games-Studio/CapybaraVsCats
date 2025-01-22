using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStartCountdown : MonoBehaviour
{
    public RawImage readyImage; // Componente RawImage para exibir o "Ready"
    public RawImage goImage; // Componente RawImage para exibir o "GO"
    public float displayTime = 1f; // Tempo de exibição de cada imagem
    public float fadeDuration = 0.5f; // Duração do fade in/out

    private bool gamePaused = true;

    void Start()
    {
        // Certifica-se de que o jogo começa pausado
        Time.timeScale = 0f;
        gamePaused = true;

        // Inicia o countdown
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        // Exibe o "Ready" com transição
        yield return StartCoroutine(FadeIn(readyImage));
        yield return new WaitForSecondsRealtime(displayTime);
        yield return StartCoroutine(FadeOut(readyImage));

        yield return new WaitForSecondsRealtime(0.5f); // Pequena pausa opcional entre "Ready" e "GO"

        // Exibe o "GO" com transição
        yield return StartCoroutine(FadeIn(goImage));
        yield return new WaitForSecondsRealtime(displayTime);
        yield return StartCoroutine(FadeOut(goImage));

        // Retorna o jogo ao normal
        Time.timeScale = 1f;
        gamePaused = false;
    }

    private IEnumerator FadeIn(RawImage image)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = 0f;
        image.color = color;
        image.gameObject.SetActive(true);

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            image.color = color;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        color.a = 1f;
        image.color = color;
    }

    private IEnumerator FadeOut(RawImage image)
    {
        float elapsedTime = 0f;
        Color color = image.color;
        color.a = 1f;
        image.color = color;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            image.color = color;
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        color.a = 0f;
        image.color = color;
        image.gameObject.SetActive(false);
    }
}
