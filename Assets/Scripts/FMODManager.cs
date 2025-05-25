using UnityEngine;
using FMODUnity;

public class FMODManager : MonoBehaviour
{
    [Header("Sons do Jogo")]
    public EventReference musicEvent; // M�sica de fundo
    public EventReference shootSound; // Som de tiro
    public EventReference menuClickSound; // Som de clique de menu

    private FMOD.Studio.EventInstance musicInstance;
    private bool isMusicPlaying = false;

    // Volumes
    private float musicVolume = 1f; // Volume da m�sica
    private float sfxVolume = 1f;   // Volume dos efeitos sonoros

    void Start()
    {
        PlayMusic(musicEvent); // Inicia a m�sica de fundo assim que o jogo come�ar
    }

    // Tocar m�sica
    public void PlayMusic(EventReference music)
    {
        if (isMusicPlaying) return; // Garante que a m�sica n�o ser� tocada mais de uma vez

        musicInstance = RuntimeManager.CreateInstance(music); // Cria a inst�ncia da m�sica
        musicInstance.start(); // Inicia a m�sica
        isMusicPlaying = true;
        SetVolume("music", musicVolume); // Aplica o volume da m�sica
    }

    // Parar a m�sica
    public void StopMusic()
    {
        if (isMusicPlaying)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            isMusicPlaying = false;
        }
    }

    // Tocar efeito sonoro
    public void PlaySoundEffect(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound); // Toca o efeito sonoro
    }

    // Ajusta o volume do �udio (m�sica ou efeito sonoro)
    public void SetVolume(string type, float volume)
    {
        if (type == "music")
        {
            musicVolume = volume;
            musicInstance.setVolume(volume); // Altera o volume da m�sica
        }
        else if (type == "sfx")
        {
            sfxVolume = volume;
            // Aqui voc� pode colocar l�gica para alterar o volume dos efeitos sonoros, se necess�rio
        }
    }

    // Fun��o para tocar o som de clique de menu
    public void PlayMenuClickSound()
    {
        PlaySoundEffect(menuClickSound);
    }
}