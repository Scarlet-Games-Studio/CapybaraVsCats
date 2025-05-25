using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    public FMODManager fmodManager;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Supondo que o botão "Fire1" seja o botão de disparo
        {
            fmodManager.PlaySoundEffect(fmodManager.shootSound); // Toca o som de tiro
        }
    }
}