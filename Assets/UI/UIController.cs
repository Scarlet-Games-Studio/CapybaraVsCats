using UnityEngine;

public class UIController : MonoBehaviour
{
    public Transform PlayerFirePoint;
    public GameObject BulletPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire()
    {
        // Cria um novo projétil na posição do PlayerFirePoint
        Instantiate(BulletPrefab, PlayerFirePoint.position, PlayerFirePoint.rotation);
    }


}
