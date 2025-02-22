using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeteorHitbox : MonoBehaviour
{
    [SerializeField] private int meteorHealth = 4;
    public SpriteRenderer sr;
    [SerializeField]private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Se colidir com o jogador
        {
            // Causa dano ao jogador
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10); // Aplica o dano de 10
            }

            // Destroi o meteoro após a colisão
            animator.SetBool("isExploding", true);
        }
        if (collision.CompareTag("PlayerProjectile")) // Se colidir com o tiro do jogador
        {

            meteorHealth--;
            StartCoroutine(Flashing());
            if(meteorHealth <= 0)
            {
                // Destroi o meteoro após a colisão
                animator.SetBool("isExploding", true);
            }
        }
    }

    //faz o sprite mudar de cor quando levar hit
    IEnumerator Flashing()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.03f);
        sr.color = Color.white;
    }
}
