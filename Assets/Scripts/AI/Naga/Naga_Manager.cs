using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Naga_Manager : MonoBehaviour
{
    public AI_FemaleNaga femaleNaga;
    public AI_MaleNaga maleNaga;
    public BossArena bossArena;
    public AudioClip bossMusic;
    public AudioClip enragedBossMusic;

    private AreaNotificationText notificationText;
    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
        notificationText = Resources.FindObjectsOfTypeAll<AreaNotificationText>()[0];
    }
    private void Start()
    {
        bossArena.Set(() => Aggro());
    }
    private void Aggro()
    {
        soundManager.PlayBossMusic(bossMusic);
        femaleNaga.Aggro();
        maleNaga.Aggro();
    }
    public void EnrageMaleNaga()
    {
        soundManager.PlayBossMusic(enragedBossMusic);
        maleNaga.Enrage();
    }
    public void EnrageFemaleNaga()
    {
        soundManager.PlayBossMusic(enragedBossMusic);
        femaleNaga.Enrage();
    }
}
