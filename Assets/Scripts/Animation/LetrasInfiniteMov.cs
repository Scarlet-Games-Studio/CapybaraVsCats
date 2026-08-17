using UnityEngine;

public class LetrasInfiniteMov : MonoBehaviour
{
    public float speed = 0.3f;
    private float parallaxHeight;
    public Transform parallaxInfinito;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        parallaxInfinito.position -= new Vector3(0, speed * Time.deltaTime, 0);
    }

    private void FixedUpdate()
    {
        parallaxHeight = parallaxInfinito.transform.position.y;
        if (parallaxHeight < -12f)
        {
            parallaxHeight = parallaxInfinito.transform.position.y;
        }
    }
}
