using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider MusicSlider;

    private void Start()
    {
        float sfxVolume;
        if (audioMixer.GetFloat("SFX", out sfxVolume))
        {
            SFXSlider.value = Mathf.Pow(10f, sfxVolume / 20f);
        }

        float musicVolume;
        if (audioMixer.GetFloat("Music", out musicVolume))
        {
            MusicSlider.value = Mathf.Pow(10f, musicVolume / 20f);
        }
    }

    public void SetSFXVolume()
    {
        float volume = Mathf.Clamp(SFXSlider.value, 0.0001f, 1f);
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }

    public void SetMusicVolume()
    {
        float volume = Mathf.Clamp(MusicSlider.value, 0.0001f, 1f);
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }
}
