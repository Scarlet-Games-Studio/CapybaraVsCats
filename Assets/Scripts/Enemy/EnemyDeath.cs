using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    [SerializeField] private GameObject nextStage;

    private void OnDestroy()
    {
        if (!Application.isPlaying) return;

        if (CompareTag("Enemy"))
        {
            EnemyHealth health = GetComponent<EnemyHealth>();
            if (health == null || health.IsDead)
                ScoreManager.AddScore(100);
        }

        if (CompareTag("Boss"))
        {
            ScoreManager.AddScore(1500);
            if (nextStage != null)
            {
                nextStage.SetActive(true);
                Time.timeScale = 0f;
            }
        }

    }
}
