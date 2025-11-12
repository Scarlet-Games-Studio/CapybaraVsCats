using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject PlayerFirePoint;
    public GameObject BulletPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerFirePoint = GameObject.Find("PlayerFirePoint");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire()
    {
        // Cria um novo projétil na posição do PlayerFirePoint
        Instantiate(BulletPrefab, PlayerFirePoint.transform.position, PlayerFirePoint.transform.rotation);
    }


}
