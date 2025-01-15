using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    private float nextFire;

    void Start()
    {
        //firePoint recebe o transform do objeto filho chamado "F"
        firePoint = transform.Find("F");
    }

    void Update()
    {
        Move();
        if (Time.time > nextFire && IsWithinScreenBounds())
        {
            Shoot();
            nextFire = Time.time + fireRate;
        }
    }

    bool IsWithinScreenBounds()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        return screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1;
    }

    void Move()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10);
            }
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        //UIManager.instance.UpdateScore(10);
        //AudioManager.instance.PlayExplosionSound();
    }
}


