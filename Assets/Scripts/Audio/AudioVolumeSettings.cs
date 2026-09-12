using System;
using UnityEngine;

/// <summary>
/// Preferências globais de áudio. FMODManager aplica master/SFX nos buses e VCAs;
/// AudioListener e VoiceVolume continuam atendendo às vozes Unity já existentes.
/// </summary>
public static class AudioVolumeSettings
{
    const string MasterKey = "Audio.MasterVolume";
    const string SfxKey = "Audio.SfxVolume";
    const string VoiceKey = "Audio.VoiceVolume";

    public static float MasterVolume => PlayerPrefs.GetFloat(MasterKey, 1f);
    public static float SfxVolume => PlayerPrefs.GetFloat(SfxKey, 1f);
    public static float VoiceVolume => PlayerPrefs.GetFloat(VoiceKey, 1f);

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyOnStartup()
    {
        AudioListener.volume = MasterVolume;
    }

    public static void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterKey, value);
        AudioListener.volume = value;
        SaveAndNotify();
    }

    public static void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value));
        SaveAndNotify();
    }

    public static void SetVoiceVolume(float value)
    {
        PlayerPrefs.SetFloat(VoiceKey, Mathf.Clamp01(value));
        SaveAndNotify();
    }

    static void SaveAndNotify()
    {
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
