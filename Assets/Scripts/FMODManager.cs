using UnityEngine;
using FMODUnity;

public class FMODManager : MonoBehaviour
{
    [Header("Sons do Jogo")]
    public EventReference musicEvent; // Música de fundo
    public EventReference shootSound; // Som de tiro
    public EventReference menuClickSound; // Som de clique de menu

    private FMOD.Studio.EventInstance musicInstance;
    private bool isMusicPlaying = false;

    // Volumes
    private float musicVolume = 1f; // Volume da música
    private float sfxVolume = 1f;   // Volume dos efeitos sonoros

    void Start()
    {
        PlayMusic(musicEvent); // Inicia a música de fundo assim que o jogo começar
    }

    // Tocar música
    public void PlayMusic(EventReference music)
    {
        if (isMusicPlaying) return; // Garante que a música não será tocada mais de uma vez

        musicInstance = RuntimeManager.CreateInstance(music); // Cria a instância da música
        musicInstance.start(); // Inicia a música
        isMusicPlaying = true;
        SetVolume("music", musicVolume); // Aplica o volume da música
    }

    // Parar a música
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

    // Ajusta o volume do áudio (música ou efeito sonoro)
    public void SetVolume(string type, float volume)
    {
        if (type == "music")
        {
            musicVolume = volume;
            musicInstance.setVolume(volume); // Altera o volume da música
        }
        else if (type == "sfx")
        {
            sfxVolume = volume;
            // Aqui você pode colocar lógica para alterar o volume dos efeitos sonoros, se necessário
        }
    }

    // Função para tocar o som de clique de menu
    public void PlayMenuClickSound()
    {
        PlaySoundEffect(menuClickSound);
    }
}