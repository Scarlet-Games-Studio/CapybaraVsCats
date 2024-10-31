using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    public Button startButton; // Drag and drop the Start button here in the inspector
    public string sceneName = "InGame"; // Name of the destination scene

    void Start()
    {
        // Add a listener to the button to call the "SwitchScene" function when the button is clicked
        startButton.onClick.AddListener(SwitchScene);
    }

    void SwitchScene()
    {
        // Switch to the destination scene
        SceneManager.LoadScene(sceneName);
    }
}
