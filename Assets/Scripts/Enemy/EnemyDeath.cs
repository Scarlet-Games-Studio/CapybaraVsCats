using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    private void OnDestroy()
    {
        if (CompareTag("Inimigo"))
            ScoreManager.AddScore(100);
    }
}
