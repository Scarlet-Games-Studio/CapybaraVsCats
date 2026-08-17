using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public Transform PlayerFirePoint;
    public GameObject BulletPrefab;

    void Awake()
    {
        // Singleton: garante que só existe 1 UIController
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // mantém entre cenas
        }
        else if (instance != this)
        {
            Destroy(gameObject); // elimina duplicados
            return;
        }

        // Inscreve para atualizar referências ao carregar cena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindToCurrentPlayer();
    }

    void BindToCurrentPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;

        PlayerFirePoint = playerController.firePoint;
        BulletPrefab = playerController.projectilePrefab;
    }

    // Método chamado pelo botão de UI
    public void Fire()
    {
        // O personagem pode ser trocado pelo CharacterSpawner depois que a cena
        // carrega. Atualiza aqui para nunca reutilizar a arma do Hiro anterior.
        BindToCurrentPlayer();

        if (BulletPrefab != null && PlayerFirePoint != null)
        {
            Instantiate(BulletPrefab, PlayerFirePoint.position, PlayerFirePoint.rotation);
        }
    }
}
