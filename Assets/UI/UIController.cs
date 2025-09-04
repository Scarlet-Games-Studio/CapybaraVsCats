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
        // Sempre que uma cena nova for carregada, tenta encontrar o PlayerFirePoint
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Assumindo que o Player tem um filho chamado "FirePoint"
            Transform firePoint = player.transform.Find("FirePoint");
            if (firePoint != null)
                PlayerFirePoint = firePoint;
        }
    }

    // Método chamado pelo botão de UI
    public void Fire()
    {
        if (BulletPrefab != null && PlayerFirePoint != null)
        {
            Instantiate(BulletPrefab, PlayerFirePoint.position, PlayerFirePoint.rotation);
        }
    }
}
