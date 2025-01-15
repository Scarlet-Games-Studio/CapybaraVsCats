using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float fireRate = 0.5f; // Tempo entre disparos
    public GameObject projectilePrefab;
    public Transform firePoint; // FirePoint normal
    public Transform firePoint2; // FirePoint extra (para power-up)
    private float nextFire;
    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        UIManager.instance.UpdateHealth(health.maxHealth);
    }

    void Update()
    {
        //Move();
        //Shoot();
        UIManager.instance.UpdateHealth(health.currentHealth); // Atualiza a UI com a saúde atual
    }

    void Move()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
            touchPosition.z = 0f; // Certifique-se de que a posição Z seja 0

            // Move o jogador em direção à posição do toque
            transform.position = Vector3.MoveTowards(transform.position, touchPosition, moveSpeed * Time.deltaTime);

            // Obtém os limites da tela
            float screenLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
            float screenRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            float screenTop = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
            float screenBottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;

            // Restringe a posição do jogador aos limites da tela
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, screenLeft, screenRight),
                Mathf.Clamp(transform.position.y, screenBottom, screenTop),
                transform.position.z
            );
        }
    }

    void Shoot()
    {
        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation); // Disparo do primeiro firePoint
            if (firePoint2 != null) // Se firePoint2 existe (após o power-up)
            {
                Instantiate(projectilePrefab, firePoint2.position, firePoint2.rotation); // Disparo do segundo firePoint
            }
            //AudioManager.instance.PlayShootSound();
        }
    }

    public void IncreaseFireRate(float amount)
    {
        fireRate -= amount;
        if (fireRate < 0.1f)
        {
            fireRate = 0.1f; // Limita a taxa de disparo mínima
        }
    }

    public void IncreaseSpeed(float amount)
    {
        moveSpeed += amount;
    }

    // Método para o power-up de BulletPower (exemplo de como aumentar firePoints)
    public void PowerUpBullet()
    {
        // Se o power-up Bullet for pego, ativa o segundo firePoint
        firePoint2 = firePoint; // No caso, colocamos o mesmo firePoint no segundo, ou podemos mudar para outra posição.
    }
}
