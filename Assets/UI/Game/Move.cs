using UnityEngine;

public class Move : MonoBehaviour
{
    public Transform background;
    public GameObject player;
    [SerializeField, Min(20f)] float joystickRadius = 70f;

    Touch activeTouch;
    Vector2 startPosition;
    int activeFingerId = -1;

    void Start() => FindPlayer();

    void OnEnable() => ResetTouch();

    void OnDisable() => ResetTouch();

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) ResetTouch();
    }

    void ResetTouch()
    {
        activeFingerId = -1;
        activeTouch = default;
    }

    void FindPlayer()
    {
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        UpdateTouch();
        if (activeFingerId == -1) return;

        Vector2 drag = activeTouch.position - startPosition;
        Vector2 input = Vector2.ClampMagnitude(drag / joystickRadius, 1f);
        PlayerController controller = player.GetComponent<PlayerController>();
        float speed = controller != null ? controller.moveSpeed : 5f;
        player.transform.position += (Vector3)(input * speed * Time.deltaTime);
        ClampPlayerToScreen();

        if (background != null)
            background.position = startPosition + Vector2.ClampMagnitude(drag, joystickRadius);
    }

    void UpdateTouch()
    {
        if (Input.touchCount == 0)
        {
            activeFingerId = -1;
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (activeFingerId == -1)
            {
                if (touch.position.x >= Screen.width * 0.5f || touch.position.y >= Screen.height * 0.5f)
                    continue;

                activeFingerId = touch.fingerId;
                activeTouch = touch;
                startPosition = touch.position;
                if (background != null) background.position = startPosition;
            }
            else if (touch.fingerId == activeFingerId)
            {
                activeTouch = touch;
            }
        }

        if (activeFingerId != -1 &&
            (activeTouch.phase == TouchPhase.Canceled || activeTouch.phase == TouchPhase.Ended))
            activeFingerId = -1;
    }

    void ClampPlayerToScreen()
    {
        Camera gameCamera = Camera.main;
        if (gameCamera == null) return;

        float depth = Mathf.Abs(player.transform.position.z - gameCamera.transform.position.z);
        Vector3 bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0.03f, 0.03f, depth));
        Vector3 topRight = gameCamera.ViewportToWorldPoint(new Vector3(0.97f, 0.97f, depth));
        Vector3 position = player.transform.position;
        position.x = Mathf.Clamp(position.x, Mathf.Min(bottomLeft.x, topRight.x), Mathf.Max(bottomLeft.x, topRight.x));
        position.y = Mathf.Clamp(position.y, Mathf.Min(bottomLeft.y, topRight.y), Mathf.Max(bottomLeft.y, topRight.y));
        player.transform.position = position;
    }
}
