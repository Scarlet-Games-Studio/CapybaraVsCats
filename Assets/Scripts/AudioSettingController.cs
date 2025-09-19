using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    public FMODManager fmodManager;
    public Slider musicVolumeSlider; // Slider para ajustar o volume da música
    public Slider sfxVolumeSlider;   // Slider para ajustar o volume dos efeitos sonoros
    public Slider dubVolumeSlider; // Slider para ajustar o volume da dublagem

    void Start()
    {
        // Define os valores iniciais dos sliders conforme os volumes atuais
        musicVolumeSlider.value = 1f;
        sfxVolumeSlider.value = 1f;
    }

    void Update()
    {
        // Atualiza os volumes conforme o jogador ajusta os sliders
        fmodManager.SetVolume("music", musicVolumeSlider.value);
        fmodManager.SetVolume("sfx", sfxVolumeSlider.value);
        fmodManager.SetVolume("dub", sfxVolumeSlider.value);
    }
}