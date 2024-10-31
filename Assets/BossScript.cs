using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    public Transform[] targets;
    public GameObject bullet;
    public float bulletSpeed = 2f;
    public bool canShoot = false;
    public int bossLife = 200;

    // Start is called before the first frame update
    void Start()
    {
        // pega os objetos que começam com "Point", Point (1), Point (2), Point (3), Point (4), Point (5)
        targets = new Transform[5];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = GameObject.Find("Point (" + (i + 1) + ")").transform;
        }

        InvokeRepeating("ShootBullets", 2f, 2f);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShootBullets()
    {
        if (!canShoot)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            // Cria uma nova bala no centro do jogador
            GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);

            // Direciona a bala para o alvo
            Vector3 direction = targets[i].position - transform.position;
            newBullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;

            // Faz o prefab olhar para a direção
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            newBullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void TakeDamage(int damage)
    {
        bossLife -= damage;

        if (bossLife <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // destrói o inimigo
            Destroy(other.gameObject);
        }
    }
}
