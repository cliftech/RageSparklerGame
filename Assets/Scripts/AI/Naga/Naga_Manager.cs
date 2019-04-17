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

    private EnemyBossHealthbar[] healthbars;
    private AreaNotificationText notificationText;
    private SoundManager soundManager;

    private bool isNagaQueenDead;
    private bool isNagaKingDead;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
        notificationText = Resources.FindObjectsOfTypeAll<AreaNotificationText>()[0];
        healthbars = Resources.FindObjectsOfTypeAll<EnemyBossHealthbar>();
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
    public void StopPlayingBossMusic()
    {
        soundManager.StopPlayingBossMusic();
    }
    public void ShowHealthbar(bool isNagaQueen)
    {
        if (isNagaQueen)
        {
            healthbars[0].Show(femaleNaga.displayName);
        }
        else
        {
            healthbars[1].Show(maleNaga.displayName);
        }
    }
    public void UpdateHealthbar(bool isNagaQueen, float health, float maxHealth)
    {
        if (isNagaQueen)
        {
            healthbars[0].UpdateHealthbar(health, maxHealth);
        }
        else
        {
            healthbars[1].UpdateHealthbar(health, maxHealth);
        }
    }
    public void HideHealthbar(bool isNagaQueen)
    {
        if (isNagaQueen)
        {
            healthbars[0].Hide();
        }
        else
        {
            healthbars[1].Hide();
        }
    }
    public void Died(bool isNagaQueen)
    {
        if (isNagaQueen)
        {
            isNagaQueenDead = true;
        }
        else
        {
            isNagaKingDead = true;
        }

        if (isNagaKingDead && isNagaQueenDead)
        {
            bossArena.OpenGate1();
            bossArena.OpenGate2();
        }
    }
}
