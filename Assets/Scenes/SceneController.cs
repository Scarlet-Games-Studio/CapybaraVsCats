using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Método para carregar uma cena
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
