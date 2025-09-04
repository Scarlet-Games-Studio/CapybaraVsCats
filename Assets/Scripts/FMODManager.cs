using UnityEngine;
using FMODUnity;

public class FMODManager : MonoBehaviour
{
    public static FMODManager instance;

    [Header("Sons do Jogo")]
    public EventReference musicEvent;      // Música de fundo
    public EventReference shootSound;      // Som de tiro
    public EventReference menuClickSound;  // Som de clique de menu

    private FMOD.Studio.EventInstance musicInstance;
    private bool isMusicPlaying = false;

    // Volumes
    private float musicVolume = 1f; 
    private float sfxVolume = 1f;   

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        PlayMusic(musicEvent); 
    }

    // Tocar música (garante que nunca duplique)
    public void PlayMusic(EventReference music)
    {
        StopMusic(); // Sempre para antes de criar outra instância

        musicInstance = RuntimeManager.CreateInstance(music);
        musicInstance.start();
        isMusicPlaying = true;
        SetVolume("music", musicVolume);
    }

    // Parar música
    public void StopMusic()
    {
        if (isMusicPlaying)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release(); // libera instância para evitar vazamentos
            isMusicPlaying = false;
        }
    }

    // Tocar efeito sonoro
    public void PlaySoundEffect(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound);
    }

    // Ajustar volume
    public void SetVolume(string type, float volume)
    {
        if (type == "music")
        {
            musicVolume = volume;
            if (isMusicPlaying)
                musicInstance.setVolume(volume);
        }
        else if (type == "sfx")
        {
            sfxVolume = volume;
        }
    }

    // Clique de menu
    public void PlayMenuClickSound()
    {
        PlaySoundEffect(menuClickSound);
    }
}
