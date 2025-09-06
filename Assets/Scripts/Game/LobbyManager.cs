using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Import necessário para TextMeshPro

public class LobbyManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text lastStageScoreText;   // Texto TMP que mostra o score da fase anterior
    public Button mapButton;              // Botão para ir ao mapa
    public Button exitButton;             // Botão para sair para o Main Menu

    [Header("Configurações do Lobby")]
    public string mapSceneName;           // Nome da cena do mapa
    public string mainMenuSceneName;      // Nome da cena do menu principal

    private void Start()
    {
        // Mostra o score do último estágio
        int lastScore = ProgressManager.GetLastStageScore();
        if (lastStageScoreText != null)
            lastStageScoreText.text = "Êxito do estágio anterior: " + lastScore;

        // Botão que leva para a cena do mapa
        if (mapButton != null)
        {
            mapButton.onClick.AddListener(() => {
                SceneManager.LoadScene(mapSceneName);
            });
        }

        // Botão que leva para o menu principal
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() => {
                SceneManager.LoadScene(mainMenuSceneName);
            });
        }
    }
}
