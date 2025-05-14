using UnityEngine;

public class BossDeath : MonoBehaviour
{
    public StageManager stageManager;

    private void OnDestroy()
    {
        if (CompareTag("Boss") && stageManager != null)
        {
            stageManager.OnStageComplete();
        }
    }
}