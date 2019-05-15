using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Necromancer : AI_Base
{
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed, projectileDamage;

    public AudioClip summonSound;
    public AudioClip projectileSummonSound;
    public AudioClip getHitSound;
    private AI_Soundmanager soundManager;

    public GameObject summonPrefab;
    public Transform summonSpawnPoint;
    public int maxSummonCount;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private LayerMask terrainMask;
    private LayerMask playerMask;
    private float landImmobalizedTime;
    private float maxAggroRange;
    private float retreatRange;
    private float minRetreatTime;
    private float maxRetreatTime;
    private float minTimeToLandTime;
    private float minProjAttackTime;
    private float maxProjAttackTime;
    private int currentSummonCount;

    private float retreatTimer;
    private float landTimer;
    private float projAttackTimer;
    private bool isGrounded;

    void Awake()
    {
        soundManager = GetComponent<AI_Soundmanager>();
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 0;
        aggroRange = 6;
        maxAggroRange = 10;
        retreatRange = 2f;
        minRetreatTime = 1.5f;
        maxRetreatTime = 3f;
        knockBackVelocity = 3;
        staggerVelocity = 2;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        landImmobalizedTime = .25f;
        minTimeToLandTime = .25f;
        minProjAttackTime = 2f;
        maxProjAttackTime = 4f;
        //-----------------------------------------------

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        SetIdle();

        stateAfterAttackCall = () => SetAggro();
        stateAfterKnockbackCall = () => SetAwakening();
        stateAfterStaggeredCall = () => SetAggro();
        stateAfterAwake = () => SetAggro();
    }
    void Update()
    {
        isGrounded = IsGrounded(terrainMask);
        if (retreatTimer > 0)
            retreatTimer -= Time.deltaTime;
        if (projAttackTimer > 0)
            projAttackTimer -= Time.deltaTime;
        if(landTimer > 0)
            landTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Aggro:
                float dist = Vector2.Distance(coll.bounds.center, target.position);
                bool groundBackwardsExists = DoesGroundForwardExists(!isDirRight, yRayLength, terrainMask, 2.5f);
                bool wallBackwardsExists = RaycastSideways_OR(!isDirRight, 5, xRayLength, terrainMask);
                float yDiff = target.position.y - coll.bounds.center.y;

                if (dist < retreatRange && retreatTimer <= 0 && groundBackwardsExists && !wallBackwardsExists)
                {
                    Retreat();
                    break;
                }
                else if (dist + .1f > maxAggroRange)
                {
                    SetIdle();
                    break;
                }

                if (target.position.x < coll.bounds.center.x)
                {
                    if (isDirRight)
                        ChangeDirection(false);
                }
                else
                {
                    if (!isDirRight)
                        ChangeDirection(true);
                }

                if (currentSummonCount < maxSummonCount)
                {
                    SummonSummon();
                    break;
                }

                if(projAttackTimer <= 0)
                    StartShootingProjectile();

                break;
            case State.Attacking:
                break;
            case State.KnockedBack:
                if (isGrounded && fullyKnockedDown && state != State.Dead)
                    EndKnockedBack();
                break;
            case State.Idle:
                if (Vector2.Distance(coll.bounds.center, target.position) < aggroRange)
                {
                    if (RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask) ||
                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask))
                    {
                        SetAggro();
                    }
                }
                break;
            case State.Dead:
                break;
            case State.Awakening:
                break;
            case State.Jumping:
                if (isGrounded && landTimer <= 0)
                    StartCoroutine(SetImmobilizeFor(landImmobalizedTime));
                break;
            case State.Immobilized:
                break;
            case State.Falling:
                if (isGrounded && landTimer <= 0)
                    SetAggro();
                break;
        }
        animator.SetBool("Is Grounded", isGrounded);
        animator.SetFloat("Vertical Velocity", rb.velocity.y);
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }

    void Retreat()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        Vector2 jumpVel = new Vector2(isDirRight ? -3 : 3, 2f);
        SetJumping(jumpVel);
        landTimer = minTimeToLandTime;
        retreatTimer = Random.Range(minRetreatTime, maxRetreatTime);
    }

    void StartShootingProjectile()
    {
        projAttackTimer = Random.Range(minProjAttackTime, maxProjAttackTime);
        SetAttack1();
    }
    void ShootProjectileEvent()
    {
        soundManager.PlayOneShot(projectileSummonSound);
        Instantiate(projectilePrefab, transform.parent).GetComponent<Projectile>()
            .Set(projectileSpawnPoint.position, new Vector2(coll.bounds.center.x < target.position.x ? 1 : -1, 0),
            "EnemyWeapon", target.position, projectileSpeed, projectileDamage);
    }

    void SummonSummon()
    {
        SetAttack2();
    }

    void SummonSummonEvent()
    {
        soundManager.PlayOneShot(summonSound);
        projAttackTimer = maxProjAttackTime;
        Instantiate(summonPrefab, summonSpawnPoint.position, Quaternion.identity, transform.parent).GetComponent<AI_Base>().SetSummon(() => SummonDiedCalledBySummon());
        currentSummonCount++;
    }
    void SummonDiedCalledBySummon()
    {
        currentSummonCount--;
    }

    void EndAttack()    // ovverides AI_Base.EndAttack() on animation events
    {
        damageContainer.SetDamageCall(() => touchDamage);
        StartCoroutine(SetImmobilizeFor(landImmobalizedTime));
    }
    IEnumerator SetImmobilizeFor(float time)
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        SetImmobilized();
        yield return new WaitForSecondsRealtime(time);
        SetAggro();
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Aggro:
                {
                    Vector2 direction;
                    if (isDirRight)
                        direction = Vector2.right;
                    else
                        direction = Vector2.left;

                    //rb.velocity = new Vector2(direction.x * movVelocity, rb.velocity.y);
                    break;
                }
            case State.Attacking:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.KnockedBack:
                break;
            case State.Idle:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Dead:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Awakening:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Immobilized:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Jumping:
                break;
            case State.Falling:
                break;
            case State.Staggered:
                if (isGrounded && landTimer <= 0)
                    rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }
    protected void GetHit(bool isRight, float damage, bool doKnockback)
    {
        if (state == State.Dead)
            return;
        health -= damage;
        if (health <= 0)
        {
            if (state != State.Dead)
                SetDead(isRight);
            target.GetComponent<Player>().AddEnemyKilldedToCount(this.GetType());
        }
        else
        {
            SetStaggered(isRight);
            landTimer = minTimeToLandTime;
        }
        soundManager.PlayOneShot(getHitSound);
        cameraController.Shake(damage);
        ParticleEffectManager.PlayEffect(ParticleEffect.Type.blood, coll.bounds.center, isRight ? Vector3.left : Vector3.right);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerWeaponTag) && state != State.KnockedBack && state != State.Dead)
        {
            var dc = other.GetComponentInParent<DamageContainer>();
            GetHit(coll.bounds.center.x < other.transform.position.x,
                dc.GetDamage(), dc.doKnockback());
        }
    }
}
