using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button startButton; // Botão Start
    public Button settingsButton; // Botão Settings
    public Button creditsButton; // Botão Credits
    public Button exitButton; // Botão Exit

    public GameObject settingsPanel; // Painel de Configurações
    public Button closeSettingsButton; // Botão para fechar o painel Settings

    public GameObject creditsPanel; // Painel de Créditos
    public Button closeCreditsButton; // Botão para fechar o painel Credits

    public string inGameSceneName = "InGame"; // Nome da cena do jogo

    private List<Button> mainMenuButtons = new List<Button>(); // Lista para armazenar os botões principais

    void Start()
    {
        // Adiciona os botões principais à lista
        mainMenuButtons.Add(startButton);
        mainMenuButtons.Add(settingsButton);
        mainMenuButtons.Add(creditsButton);
        mainMenuButtons.Add(exitButton);

        // Configura os listeners dos botões principais
        if (startButton != null)
            startButton.onClick.AddListener(() => StartGame());

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => OpenPanel(settingsPanel, closeSettingsButton));

        if (creditsButton != null)
            creditsButton.onClick.AddListener(() => OpenPanel(creditsPanel, closeCreditsButton));

        if (exitButton != null)
            exitButton.onClick.AddListener(() => ExitGame());

        // Configura os listeners dos botões de fechar
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(() => ClosePanel(settingsPanel, closeSettingsButton));

        if (closeCreditsButton != null)
            closeCreditsButton.onClick.AddListener(() => ClosePanel(creditsPanel, closeCreditsButton));
    }

    // Carrega a cena do jogo
    void StartGame()
    {
        SceneManager.LoadScene(inGameSceneName);
    }

    // Abre um painel e mostra o botão de fechar correspondente
    void OpenPanel(GameObject panel, Button closeButton)
    {
        if (panel != null && closeButton != null)
        {
            panel.SetActive(true); // Ativa o painel
            closeButton.gameObject.SetActive(true); // Torna o botão de fechar visível
            SetMainMenuButtonsActive(false); // Desativa os botões principais
        }
        else
        {
            Debug.LogWarning("Painel ou botão de fechar não configurado no Inspetor.");
        }
    }

    // Fecha um painel e esconde o botão de fechar correspondente
    void ClosePanel(GameObject panel, Button closeButton)
    {
        if (panel != null && closeButton != null)
        {
            panel.SetActive(false); // Desativa o painel
            closeButton.gameObject.SetActive(false); // Esconde o botão de fechar
            SetMainMenuButtonsActive(true); // Reativa os botões principais
        }
        else
        {
            Debug.LogWarning("Painel ou botão de fechar não configurado no Inspetor.");
        }
    }

    // Sai do jogo
    void ExitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }



    // Ativa ou desativa os botões principais do menu
    void SetMainMenuButtonsActive(bool isActive)
    {
        foreach (Button button in mainMenuButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(isActive); // Ativa ou desativa o GameObject do botão
            }
        }
    }
}
