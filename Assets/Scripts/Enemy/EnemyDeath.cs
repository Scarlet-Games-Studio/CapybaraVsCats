using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    private void OnDestroy()
    {
        if (CompareTag("Enemy"))
        {
            ScoreManager.AddScore(100);
        }

        if (CompareTag("Boss"))
        {
            ScoreManager.AddScore(1500);
            stageManager.OnStageComplete();
        }
    }

}
