using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SpeedIncrease : MonoBehaviour
{
    private ParallaxScroller parallax;
    public AnimationCurve speedCurve;
    public float time;
    private bool isActive;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Pega a referencia o Parallax scroller
        parallax = GameObject.Find("Cenário Parallax").GetComponent<ParallaxScroller>();
    }

    // Update is called once per frame
    void Update()
    {
        //aumenta a velocidade em uma curva
        if (isActive)
        {
            time += Time.deltaTime;
            parallax.speed = speedCurve.Evaluate(time);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isActive = true;
        }
    }
}
