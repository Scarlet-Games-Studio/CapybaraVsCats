using UnityEngine;

// Adaptador mantido com o GUID da antiga integração FMOD.
// O FMOD foi removido do projeto, mas cenas antigas ainda referenciam este componente.
public class FMODManager : MonoBehaviour
{
    public static FMODManager instance;

    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip menuClickSound;
    float sfxVolume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (musicSource == null) musicSource = GetComponent<AudioSource>();
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    public void SetVolume(string type, float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (type == "music" && musicSource != null) musicSource.volume = volume;
        if (type == "sfx" || type == "dub") sfxVolume = volume;
    }

    public void PlayMenuClickSound()
    {
        if (musicSource != null && menuClickSound != null)
            musicSource.PlayOneShot(menuClickSound, sfxVolume);
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
