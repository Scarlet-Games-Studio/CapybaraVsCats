using UnityEngine;

public class Move : MonoBehaviour
{
    public Touch t;
    public Vector2 startPos;
    public Transform background;
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        t = new Touch { fingerId = -1 };
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            for (int a = 0; a < Input.touchCount; a++)
            {
                if(t.fingerId == -1)
                {
                    if(Input.GetTouch(a).position.x < Screen.width / 2 && Input.GetTouch(a).position.y < Screen.height / 2)
                    {
                        t = Input.GetTouch(a);
                        startPos = t.position;
                        background.position = startPos;
                    }
                }
                else
                {
                    if(Input.GetTouch(a).fingerId == t.fingerId)
                    {
                        t = Input.GetTouch(a);
                    }
                }
            }

            if(t.fingerId != -1)
            {
                if(t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
                {
                    t = new Touch { fingerId = -1 };
                }
                else
                {
                    Vector2 dist = t.position - startPos;
                    Vector3 newPosition = startPos + Vector2.ClampMagnitude(dist, 70);
                    transform.position = newPosition;
                    //player.transform.position += (Vector3)dist * 0.002f * Time.deltaTime;

                    // obter os limites da tela
                    float screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x + 0.1f;
                    float screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - 0.1f;
                    float screenTop = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y - 0.1f;
                    float screenBottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y + 0.1f;

                    // limitar a movimentação do player
                    if (player.transform.position.x < screenLeft)
                    {
                        player.transform.position = new Vector2(screenLeft, player.transform.position.y);
                    }
                    else if (player.transform.position.x > screenRight)
                    {
                        player.transform.position = new Vector2(screenRight, player.transform.position.y);
                    }

                    if (player.transform.position.y < screenBottom)
                    {
                        player.transform.position = new Vector2(player.transform.position.x, screenBottom);
                    }
                    else if (player.transform.position.y > screenTop)
                    {
                        player.transform.position = new Vector2(player.transform.position.x, screenTop);
                    }

                    player.transform.position += (Vector3)dist * 0.002f * Time.deltaTime;


                }
            }
        }
        
    }
}
