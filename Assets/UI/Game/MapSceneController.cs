using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSceneController : MonoBehaviour
{
    [Header("Planetas disponíveis")]
    public Button earthButton;
    public Button marsButton;
    [Header("Navegação")]
    public Button lobbyButton;
    public Button exitButton;
    public Button[] comingSoonButtons;

    [Header("Cenas")]
    public string earthScene = "ingame";
    public string marsScene = "stage2";
    public string comingSoonScene = "ComingSoon";
    public string lobbyScene = "Lobby";
    public string mainMenuScene = "MainMenu";

    void Start()
    {
        Bind(earthButton, earthScene);
        Bind(marsButton, marsScene);
        Bind(lobbyButton, lobbyScene);
        Bind(exitButton, mainMenuScene);
        if (comingSoonButtons != null)
            foreach (Button button in comingSoonButtons) Bind(button, comingSoonScene);
    }

    static void Bind(Button button, string scene)
    {
        if (button == null || string.IsNullOrWhiteSpace(scene)) return;
        button.onClick.AddListener(() =>
        {
            if (Application.CanStreamedLevelBeLoaded(scene)) SceneManager.LoadScene(scene);
            else Debug.LogError($"Cena '{scene}' não está habilitada no Build Settings.");
        });
    }
}
