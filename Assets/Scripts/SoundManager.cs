using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip bgmClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        audioSource.loop = true;
        audioSource.Stop();
    }
    public void PlayMusic(AudioClip audioClip, bool forceRestart = false)
    {
        if (forceRestart || audioClip != audioSource.clip)
        {
            audioSource.clip = bgmClip = audioClip;
            audioSource.Play();
        }
    }
    public void PlayBossMusic(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
    public void StopPlayingBossMusic()
    {
        if (bgmClip != null)
        {
            audioSource.clip = bgmClip;
            audioSource.Play();
        }
        else
            audioSource.Stop();
    }
    public void PauseMusic()
    {
        audioSource.Pause();
    }
    public void ResumeMusic()
    {
        audioSource.UnPause();
    }
}
