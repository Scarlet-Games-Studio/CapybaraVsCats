using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [SerializeField] private GameObject nextStage;

    private void OnDestroy()
    {
        if (CompareTag("Enemy"))
        {
            ScoreManager.AddScore(100);
        }

        if (CompareTag("Boss"))
        {
            ScoreManager.AddScore(1500);
            nextStage.SetActive(true);
            Time.timeScale = 0f;
        }

    }
}
