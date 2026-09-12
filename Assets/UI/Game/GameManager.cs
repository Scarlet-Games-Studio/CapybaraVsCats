using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-2000)]
public class GameManager : MonoBehaviour
{
    public enum GameplayState { Playing, GameOver, StageComplete }

    public static GameManager instance;
    public GameplayState State { get; private set; } = GameplayState.Playing;
    public bool IsPlaying => State == GameplayState.Playing;

    void Awake()
    {
        if (instance != null && instance != this && instance.gameObject.scene == gameObject.scene)
        {
            Destroy(this);
            return;
        }

        instance = this;
        ResetRuntimeState();
    }

    void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
    }

    void ResetRuntimeState()
    {
        State = GameplayState.Playing;
        Time.timeScale = 1f;
        FMODManager.ExitPausedState();
    }

    public void GameOver()
    {
        if (!IsPlaying) return;
        State = GameplayState.GameOver;
        FMODManager.EnterGameOverAudio();

        GameOverScreen screen = FindAnyObjectByType<GameOverScreen>(FindObjectsInactive.Include);
        if (screen != null)
            screen.ShowGameOverScreen();
        else
        {
            Time.timeScale = 0f;
            Debug.LogWarning("GameOverScreen não foi encontrado na cena.");
        }
    }

    public void CompleteStage()
    {
        if (!IsPlaying) return;
        State = GameplayState.StageComplete;
        FMODManager.EnterStageCompleteAudio();

        StageManager stage = FindAnyObjectByType<StageManager>(FindObjectsInactive.Include);
        if (stage != null)
            stage.OnStageComplete();
        else
        {
            Time.timeScale = 0f;
            Debug.LogWarning("StageManager não foi encontrado ao concluir a fase.");
        }
    }

    public void ContinueAfterReward()
    {
        if (State != GameplayState.GameOver) return;
        State = GameplayState.Playing;
        Time.timeScale = 1f;
        FMODManager.ExitPausedState();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
