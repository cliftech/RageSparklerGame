﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_FemaleNaga : AI_Base
{
    private AI_Soundmanager sound;
    private SoundManager soundManager;
    private AreaNotificationText notificationText;
    private EnemyBossHealthbar bossHealthbar;
    private LayerMask terrainMask;
    private LayerMask playerMask;

    public AudioClip music;
    public AudioClip rangedAttackHitSound;
    public AudioClip pierceAttackHitSound;
    public AudioClip getHitSound;
    public AudioClip moveSound;
    public AudioClip shoutLoopSound;

    public float rangedAttackDamage;
    private float rangedAttackRange;
    public float pierceAttackDamage;
    private float pierceAttackRange;
    private float timeBetweenRanged;
    private float staggerCounter;
    private float staggerFalloff;
    private int maxStaggerCount;

    public string displayName = "Queen Naga";
    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private bool isNextPierceAttack;

    void Awake()
    {
        sound = GetComponent<AI_Soundmanager>();
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        notificationText = GameObject.FindObjectOfType<AreaNotificationText>();
        bossHealthbar = GameObject.FindObjectOfType<EnemyBossHealthbar>();
        Initialize();
    }
    void Start()
    {
        movVelocity = 5;
        aggroRange = 5;
        rangedAttackRange = 1;
        pierceAttackRange = 2;
        staggerVelocity = 0.5f;
        staggerCounter = 0;
        staggerFalloff = 1;
        maxStaggerCount = 3;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        SetIdle();

        stateAfterAttackCall = () => SetAggro();
        stateAfterStaggeredCall = () => SetAggro();
    }

    void Update()
    {
        switch (state)
        {
            case State.Running:
                if (RaycastSideways_OR(isDirRight, 3, xRayLength, terrainMask))
                {
                    Teleport();
                }
                break;
            case State.Aggro:
                bool inLineWithPlayer = RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask) ||
                                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask);
                float dist = Vector2.Distance(transform.position, target.position);
                if (dist <= pierceAttackRange)
                {
                    AttackPierce();
                }
                else
                {
                    AttackRanged();
                }
                break;
            case State.Idle:
                if (Vector2.Distance(transform.position, target.position) < aggroRange)
                {
                    if (RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask) ||
                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask))
                    {
                        notificationText.ShowNotification(displayName);
                        soundManager.PlayBossMusic(music);
                        ChangeDirection(coll.bounds.center.x < target.position.x);
                        bossHealthbar.Show();
                        bossHealthbar.UpdateHealthbar(health, maxHealth);
                        SetAggro();
                    }
                }
                break;


        }
    }
    void AttackRanged()
    {
        SetAttack1();
    }
    void AttackPierce()
    {
        damageContainer.SetDamageCall(() => pierceAttackDamage);
        SetAttack2();
    }
    void EndAttackRanged()
    {
        SetAggro();
    }
    void EndAttackPierce()
    {
        damageContainer.SetDamageCall(() => touchDamage);
        SetRunning();
    }
    void Teleport()
    {
        Vector2 leftHitPoint = Physics2D.Raycast(coll.bounds.center, Vector2.left, 1000, terrainMask).point;
        Vector2 rightHitPoint = Physics2D.Raycast(coll.bounds.center, Vector2.right, 1000, terrainMask).point;
        Vector2 midPoint = (leftHitPoint + rightHitPoint) / 2;
        Vector2 position = Physics2D.Raycast(midPoint, Vector2.down, 1000, terrainMask).point;
        Vector2 offset = (Vector3.up * coll.bounds.extents.y) + transform.position - coll.bounds.center;
        transform.position = position + offset;
    }
    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Running:
                if (target.position.x < coll.bounds.center.x)
                {
                    if (isDirRight)
                    {
                        ChangeDirection(false);
                    }
                }
                else
                {
                    if (!isDirRight)
                    {
                        ChangeDirection(true);
                    }
                }
                Vector2 direction;
                if (isDirRight)
                {
                    direction = Vector2.left;
                }
                else
                {
                    direction = Vector2.right;
                }
                rb.velocity = new Vector2(direction.x * movVelocity, rb.velocity.y);
                break;
            case State.Aggro:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Attacking:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Idle:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Dead:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }
    protected void GetHit(bool isRight, float damage)
    {
        if (state == State.Dead)
            return;
        health -= damage;
        if (health <= 0)
        {
            SetDead(isRight);
            StopAllCoroutines();
            soundManager.StopPlayingBossMusic();
            bossHealthbar.Hide();
        }
        else
        {
            bool stagger = false;
            if (staggerCounter + 1 <= maxStaggerCount)
                stagger = true;

            if (stagger)
            {
                staggerCounter += 1;
                SetStaggered(isRight);
            }
        }
        bossHealthbar.UpdateHealthbar(health, maxHealth);
        sound.PlayOneShot(getHitSound);
        cameraController.Shake(damage);
        ParticleEffectManager.PlayEffect(ParticleEffect.Type.blood, coll.bounds.center, isRight ? Vector3.left : Vector3.right);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerWeaponTag) && state != State.Dead)
        {
            DamageContainer damageContainer = other.GetComponentInParent<DamageContainer>();
            GetHit(transform.position.x < other.transform.position.x,
                damageContainer.GetDamage());
        }
    }
    void PlayRangedAttackEffect()
    {
        cameraController.Shake(3);
    }
    void PlayPierceAttackEffect()
    {
        cameraController.Shake(3);
    }

}