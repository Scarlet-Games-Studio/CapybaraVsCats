using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParallaxScroller : MonoBehaviour
{
    public float speed = 0.3f; // Velocidade do movimento no eixo Y
    public Transform parallaxController; //Objeto que fará o efeito parallax
    public Transform parallaxInfinito;//Objeto que será loopado no parallax
    [SerializeField]private float parallaxHeight; //Variavel que define a altura do parallax para looping

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        parallaxController.position -= new Vector3(0, speed * Time.deltaTime, 0);
    }
    private void FixedUpdate()
    {
        parallaxHeight = parallaxInfinito.transform.position.y;
        if (parallaxHeight < -12f)
        {
            parallaxInfinito.transform.position = new Vector2(0, 12f);
        }
    }
}