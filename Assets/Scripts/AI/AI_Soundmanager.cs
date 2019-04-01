using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Soundmanager : MonoBehaviour
{
    public AudioSource oneShotAudioSource;

    public void PlayOneShot(AudioClip clip)
    {
        oneShotAudioSource.PlayOneShot(clip);
    }
}
