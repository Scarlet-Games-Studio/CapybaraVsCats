using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public Transform PlayerFirePoint;
    public GameObject BulletPrefab;

    void Awake()
    {
        if (instance != null && instance != this && instance.gameObject.scene == gameObject.scene)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    void Start() => BindToCurrentPlayer();

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void BindToCurrentPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            PlayerFirePoint = null;
            BulletPrefab = null;
            return;
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;
        PlayerFirePoint = playerController.firePoint;
        BulletPrefab = playerController.projectilePrefab;
    }

    public void Fire()
    {
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;
        BindToCurrentPlayer();
        GameObject player = GameObject.FindWithTag("Player");
        PlayerController playerController = player != null ? player.GetComponent<PlayerController>() : null;
        if (playerController != null) playerController.Shoot();
    }
}
