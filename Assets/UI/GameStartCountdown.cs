using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStartCountdown : MonoBehaviour
{
    public GameObject panelSelection; // Painel onde estão os 3 botões
    public Button[] selectionButtons; // Array com os 3 botões
    public RawImage readyImage;
    public RawImage goImage;
    public float displayTime = 1f;
    public float fadeDuration = 0.5f;

    private bool gamePaused = true;

    void Start()
    {
        Time.timeScale = 0f;
        gamePaused = true;

        // Mostra o painel de seleção
        panelSelection.SetActive(true);

        // Associa o clique de todos os botões
        foreach (Button btn in selectionButtons)
        {
            btn.onClick.AddListener(() => OnSelectionButtonClicked(btn));
        }
    }

    private void OnSelectionButtonClicked(Button clickedButton)
    {
        // Aqui você pode guardar qual botão foi clicado, se precisar
        Debug.Log("Botão selecionado: " + clickedButton.name);

        // Esconde o painel de seleção
        panelSelection.SetActive(false);

        // Começa o countdown
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
