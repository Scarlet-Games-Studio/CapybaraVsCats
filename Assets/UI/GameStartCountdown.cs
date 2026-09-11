using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStartCountdown : MonoBehaviour
{
    public RawImage readyImage;
    public RawImage goImage;
    public float displayTime = 1f;
    public float fadeDuration = 0.5f;

    void Start()
    {
        if (readyImage == null || goImage == null)
        {
            Debug.LogWarning("Countdown sem imagens configuradas; iniciando o jogo sem bloqueio.", this);
            Time.timeScale = 1f;
            enabled = false;
            return;
        }

        readyImage.raycastTarget = false;
        goImage.raycastTarget = false;
        Time.timeScale = 0f;

        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        yield return StartCoroutine(FadeIn(readyImage));
        yield return new WaitForSecondsRealtime(displayTime);
        yield return StartCoroutine(FadeOut(readyImage));

        yield return new WaitForSecondsRealtime(0.5f);

        yield return StartCoroutine(FadeIn(goImage));
        yield return new WaitForSecondsRealtime(displayTime);
        yield return StartCoroutine(FadeOut(goImage));

        if (GameManager.instance == null || GameManager.instance.IsPlaying)
            Time.timeScale = 1f;
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
