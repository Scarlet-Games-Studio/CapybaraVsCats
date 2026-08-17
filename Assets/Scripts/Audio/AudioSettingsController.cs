using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    public FMODManager fmodManager;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider dubVolumeSlider;

    void Start()
    {
        if (musicVolumeSlider != null) musicVolumeSlider.value = 1f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
        if (dubVolumeSlider != null) dubVolumeSlider.value = 1f;
    }

    void Update()
    {
        if (fmodManager == null) return;
        if (musicVolumeSlider != null) fmodManager.SetVolume("music", musicVolumeSlider.value);
        if (sfxVolumeSlider != null) fmodManager.SetVolume("sfx", sfxVolumeSlider.value);
        if (dubVolumeSlider != null) fmodManager.SetVolume("dub", dubVolumeSlider.value);
    }
}
