using UnityEngine;
using UnityEngine.SceneManagement;

public class Move : MonoBehaviour
{
    public Touch t;
    public Vector2 startPos;
    public Transform background;
    public GameObject player;

    void Awake()
    {
        // Garante que sempre reinicia o touch
        t = new Touch { fingerId = -1 };

        // Inscreve no evento de cena carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Remove do evento quando o objeto for destruído
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        FindPlayer();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sempre que uma cena nova for carregada, reprocura o Player
        FindPlayer();
    }

    void FindPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
    }

    void Update()
    {
        if (player == null) return; // Se o player não existir, não faz nada

        if (Input.touchCount > 0)
        {
            for (int a = 0; a < Input.touchCount; a++)
            {
                if (t.fingerId == -1)
                {
                    if (Input.GetTouch(a).position.x < Screen.width / 2 &&
                        Input.GetTouch(a).position.y < Screen.height / 2)
                    {
                        t = Input.GetTouch(a);
                        startPos = t.position;
                        if (background != null)
                            background.position = startPos;
                    }
                }
                else
                {
                    if (Input.GetTouch(a).fingerId == t.fingerId)
                    {
                        t = Input.GetTouch(a);
                    }
                }
            }

            if (t.fingerId != -1)
            {
                if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
                {
                    t = new Touch { fingerId = -1 };
                }
                else
                {
                    Vector2 dist = t.position - startPos;
                    Vector3 newPosition = startPos + Vector2.ClampMagnitude(dist, 70);
                    transform.position = newPosition;

                    // Limites da tela
                    float screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x + 0.1f;
                    float screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - 0.1f;
                    float screenTop = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y - 0.1f;
                    float screenBottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 0.1f;

                    // Limitar movimentação do player
                    Vector2 clampedPosition = player.transform.position;
                    clampedPosition.x = Mathf.Clamp(clampedPosition.x, screenLeft, screenRight);
                    clampedPosition.y = Mathf.Clamp(clampedPosition.y, screenBottom, screenTop);
                    player.transform.position = clampedPosition;

                    // Movimentação do player
                    player.transform.position += (Vector3)dist * 0.005f * Time.deltaTime;
                }
            }
        }
    }
}
