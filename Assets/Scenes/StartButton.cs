using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    // Método chamado quando o botão Start é pressionado
    public void OnStartButtonPressed()
    {
        // Nome da cena para a qual queremos ir
        string sceneName = "iNGAME";

        // Carrega a cena do jogo
        SceneManager.LoadScene(sceneName);
    }
}
