using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    public FMODManager fmodManager;
    [Tooltip("Controle de volume geral (rotulado SOM no menu).")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider dubVolumeSlider;

    void OnEnable()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(AudioVolumeSettings.MasterVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(AudioVolumeSettings.SfxVolume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        if (dubVolumeSlider != null)
        {
            dubVolumeSlider.SetValueWithoutNotify(AudioVolumeSettings.VoiceVolume);
            dubVolumeSlider.onValueChanged.AddListener(SetVoiceVolume);
        }
    }

    void OnDisable()
    {
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
        if (dubVolumeSlider != null) dubVolumeSlider.onValueChanged.RemoveListener(SetVoiceVolume);
    }

    public void SetMasterVolume(float value) => AudioVolumeSettings.SetMasterVolume(value);
    public void SetSfxVolume(float value) => AudioVolumeSettings.SetSfxVolume(value);
    public void SetVoiceVolume(float value) => AudioVolumeSettings.SetVoiceVolume(value);
}
