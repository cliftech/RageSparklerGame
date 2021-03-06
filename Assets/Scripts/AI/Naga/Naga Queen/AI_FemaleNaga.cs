﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_FemaleNaga : AI_Base
{
    private Naga_Manager nagaManager;
    private AI_Soundmanager sound;
    private LayerMask terrainMask;
    private LayerMask playerMask;

    public GameObject explosionPrefab;
    public GameObject enragedPrefab;

    public AudioClip rangedAttackHitSound;
    public AudioClip pierceAttackHitSound;
    public AudioClip getHitSound;

    private Transform originalParent;

    public bool startFacingLeft;
    public float staffAttackDamage;
    public float rangedAttackDamage;
    public float pierceAttackDamage;
    private float rangedAttackRange;
    private float pierceAttackRange;
    private float timeBetweenRanged;
    private float staggerCounter;
    private float staggerFalloff;
    private float maxRunningDistance;
    private int maxStaggerCount;
    private bool canRangedAttack = true;
    public string displayName = "Queen Naga";
    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    void Awake()
    {
        nagaManager = transform.parent.GetComponent<Naga_Manager>();
        originalParent = transform.parent;
        sound = GetComponent<AI_Soundmanager>();
        Initialize();
    }
    void Start()
    {
        movVelocity = 2;
        aggroRange = 5;
        rangedAttackRange = 50;
        pierceAttackRange = 2;
        timeBetweenRanged = 1;
        staggerVelocity = 0.5f;
        staggerCounter = 0;
        staggerFalloff = 1;
        maxStaggerCount = 3;
        maxRunningDistance = 3;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        SetIdle();

        stateAfterAttackCall = () => SetAggro();
        stateAfterStaggeredCall = () => SetAggro();
        if (!startFacingLeft)
        {
            ChangeDirection(true);
        }
    }

    void Update()
    {
        if (staggerCounter > 0)
            staggerCounter -= Time.deltaTime * staggerFalloff;

        float dist = Vector2.Distance(transform.position, target.position);
        switch (state)
        {
            case State.Running:
                if (dist > maxRunningDistance)
                {
                    SetAggro();
                }
                else if (RaycastSideways_OR(!isDirRight, 3, xRayLength, terrainMask))
                {
                    Teleport();
                }
                break;
            case State.Aggro:
                bool inLineWithPlayer = RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask) ||
                                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask);
                if (dist <= pierceAttackRange)
                {
                    AttackPierce();
                }
                else if (dist <= rangedAttackRange && canRangedAttack)
                {
                    AttackRanged();
                }
                break;
            case State.Idle:
                break;
        }
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }
    void AttackRanged()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        damageContainer.SetDamageCall(() => staffAttackDamage);
        SetAttack1();
    }
    void AttackPierce()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        damageContainer.SetDamageCall(() => pierceAttackDamage);
        SetAttack2();
    }
    void EndAttackRanged()
    {
        StartCoroutine(DisableRanged(timeBetweenRanged));
        SetAggro();
    }
    private IEnumerator DisableRanged(float time)
    {
        canRangedAttack = false;
        yield return new WaitForSecondsRealtime(time);
        canRangedAttack = true;
    }
    void EndAttackPierce()
    {
        damageContainer.SetDamageCall(() => touchDamage);
        SetRunning();
    }
    void Teleport()
    {
        animator.SetTrigger("Teleport");
        SetImmobilized();
    }
    void TeleportToMiddle()
    {
        Vector2 leftHitPoint = Physics2D.Raycast(coll.bounds.center, Vector2.left, 1000, terrainMask).point;
        Vector2 rightHitPoint = Physics2D.Raycast(coll.bounds.center, Vector2.right, 1000, terrainMask).point;
        Vector2 midPoint = (leftHitPoint + rightHitPoint) / 2;
        Vector2 position = Physics2D.Raycast(midPoint, Vector2.down, 1000, terrainMask).point;
        Vector2 offset = (Vector3.up * coll.bounds.extents.y) + transform.position - coll.bounds.center;
        transform.position = position + offset;
    }
    void EndTeleport()
    {
        SetAggro();
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
            case State.Immobilized:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }
    public void Aggro()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        nagaManager.ShowHealthbar(true);
        nagaManager.UpdateHealthbar(true, health, maxHealth);
        SetAggro();
    }
    public void Enrage()
    {
        state = State.Dead;
        rb.velocity = Vector2.zero;
        TeleportToMiddle();
        animator.SetTrigger("Transform");
        StartCoroutine(StartIncreasingScale(0.5f, 2f, 2f));
    }
    private void EnterRageState()
    {
        Instantiate(enragedPrefab, transform.position, Quaternion.identity, transform.parent);
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private IEnumerator StartIncreasingScale(float delay, float time, float scale)
    {
        yield return new WaitForSecondsRealtime(delay);
        Vector2 targetScale = new Vector2(isDirRight ? -scale : scale, scale);
        while (transform.localScale.x < 2)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, targetScale, Time.deltaTime / time);
            yield return null;
        }
        transform.localScale = targetScale;
    }
    protected void GetHit(bool isRight, float damage)
    {
        if (state == State.Dead)
            return;
        health -= damage;
        if (health <= 0)
        {
            StopAllCoroutines();
            nagaManager.EnrageMaleNaga();
            SetDead(isRight, this.GetType(), 0);
            nagaManager.Died(true);
            nagaManager.HideHealthbar(true);
            this.enabled = false;
            target.GetComponent<Player>().AddEnemyKilledToCount(this.GetType());
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
            nagaManager.UpdateHealthbar(true, health, maxHealth);
        }
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
        sound.PlayOneShot(rangedAttackHitSound);
    }
    void PlayPierceAttackEffect()
    {
        cameraController.Shake(3);
        sound.PlayOneShot(pierceAttackHitSound);
    }
    void SpawnExplosion()
    {
        Instantiate(explosionPrefab, target.position, Quaternion.identity, originalParent).GetComponent<NagaQueenExplosionEffect>().Set(target.position, rangedAttackDamage);
    }
}
