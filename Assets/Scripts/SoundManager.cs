﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        audioSource.loop = true;
        audioSource.Stop();
    }
    public void PlayMusic(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
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