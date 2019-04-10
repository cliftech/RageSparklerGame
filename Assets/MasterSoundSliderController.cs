using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterSoundSliderController : MonoBehaviour
{
    public UnityEngine.UI.Slider slider;
    // Start is called before the first frame update
    void Start()
    {
        slider.value = PlayerPrefs.HasKey("MasterVolume")?PlayerPrefs.GetFloat("MasterVolume"):1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
