using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MasterSoundSliderController : MonoBehaviour
{
    public AudioMixer audioMixer;

    public UnityEngine.UI.Slider masterSlider;
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider soundsSlider;

    void Start()
    {
        SetMasterVolume(PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : 0);
        SetSoundVolume(PlayerPrefs.HasKey("SoundsVolume") ? PlayerPrefs.GetFloat("SoundsVolume") : 0);
        SetMusicVolume(PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 0);
    }

    public void SetMasterVolume(float vol)
    {
        masterSlider.value = vol;
        audioMixer.SetFloat("MasterVolume", vol * 5);
        PlayerPrefs.SetFloat("MasterVolume", vol);
    }
    public void SetSoundVolume(float vol)
    {
        soundsSlider.value = vol;
        audioMixer.SetFloat("SoundsVolume", vol * 5);
        PlayerPrefs.SetFloat("SoundsVolume", vol);
    }
    public void SetMusicVolume(float vol)
    {
        musicSlider.value = vol;
        audioMixer.SetFloat("MusicVolume", vol * 5);
        PlayerPrefs.SetFloat("MusicVolume", vol);
    }
}
