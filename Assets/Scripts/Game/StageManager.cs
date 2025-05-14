using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public GameObject stageCompleteUI;
    public Button nextButton;
    public Button exitButton;
    public string nextSceneName;

    private void Start()
    {
        stageCompleteUI.SetActive(false);
        nextButton.interactable = false;

        exitButton.onClick.AddListener(() => {
            SceneManager.LoadScene("MainMenu");
        });

        nextButton.onClick.AddListener(() => {
            ProgressManager.SaveProgress();
            SceneManager.LoadScene(nextSceneName);
        });
    }

    public void OnStageComplete()
    {
        stageCompleteUI.SetActive(true);
        ProgressManager.SaveProgress();
    }

    public void EnableNextButton()
    {
        nextButton.interactable = true;
    }
}