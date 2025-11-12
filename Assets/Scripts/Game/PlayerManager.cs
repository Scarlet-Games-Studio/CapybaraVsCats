using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static int characterID; // ID 1 = Hiro, ID 2 = Mika, ID 1 = Jack
    public GameObject Hiro;
    public GameObject Mika;
    public GameObject Jack;
    public Transform spawnpoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (characterID == 1)
        {
            Instantiate(Hiro, spawnpoint);
        }
        else if(characterID == 2)
        {
            Instantiate(Mika, spawnpoint);
        }
        else if (characterID == 3)
        {
            Instantiate(Jack, spawnpoint);
        }
        else
        {
            return;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
