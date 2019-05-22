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

    void OnEnable()
    {
        Settings settings = SaveManager.LoadSettings();

        SetMasterVolume(settings.masterVolumeSliderValue);
        SetSoundVolume(settings.soundfxVolumeSliderValue);
        SetMusicVolume(settings.musicVolumeSliderValue);
    }

    public void SetMasterVolume(float vol)
    {
        Settings settings = SaveManager.LoadSettings();
        masterSlider.value = vol;
        audioMixer.SetFloat("MasterVolume", vol * 5);
        settings.masterVolumeSliderValue = vol;
        SaveManager.SaveSettings(settings);
    }
    public void SetSoundVolume(float vol)
    {
        Settings settings = SaveManager.LoadSettings();
        soundsSlider.value = vol;
        audioMixer.SetFloat("SoundsVolume", vol * 5);
        settings.soundfxVolumeSliderValue = vol;
        SaveManager.SaveSettings(settings);
    }
    public void SetMusicVolume(float vol)
    {
        Settings settings = SaveManager.LoadSettings();
        musicSlider.value = vol;
        audioMixer.SetFloat("MusicVolume", vol * 5);
        settings.musicVolumeSliderValue = vol;
        SaveManager.SaveSettings(settings);
    }
}
