using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterSoundSliderController : MonoBehaviour
{
    public UnityEngine.UI.Slider masterSlider;
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider soundsSlider;
    // Start is called before the first frame update
    void Start()
    {
        masterSlider.value = PlayerPrefs.HasKey("MasterVolume") ? PlayerPrefs.GetFloat("MasterVolume") : 1;
        musicSlider.value = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 1;
        soundsSlider.value = PlayerPrefs.HasKey("SoundsVolume") ? PlayerPrefs.GetFloat("SoundsVolume") : 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetMasterVolume(float vol)
    {
        masterSlider.value = vol;
        PlayerPrefs.SetFloat("MasterVolume", vol);
        SetSoundVolume(vol);
        SetMusicVolume(vol);
    }
    public void SetSoundVolume(float vol)
    {
        soundsSlider.value = vol;
        PlayerPrefs.SetFloat("SoundsVolume", vol);
    }
    public void SetMusicVolume(float vol)
    {
        musicSlider.value = vol;
        PlayerPrefs.SetFloat("MusicVolume", vol);
    }
}
