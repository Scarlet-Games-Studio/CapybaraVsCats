using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Checa se o projétil colidiu com algo (você pode filtrar por tag se precisar)
        Destroy(gameObject);
    }
}
