using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Necromancer : AI_Base
{
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed, projectileDamage;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private LayerMask terrainMask;
    private LayerMask playerMask;
    private float landImmobalizedTime;
    private float maxAggroRange;
    private float retreatRange;
    private float minRetreatTime;
    private float maxRetreatTime;
    private bool isNextDoubleAttack;
    private float minTimeToLandTime;

    private float retreatTimer;
    private float minTimeToLandTimer;
    private bool isGrounded;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 0;
        aggroRange = 3;
        maxAggroRange = 5;
        retreatRange = 2f;
        minRetreatTime = 1.5f;
        maxRetreatTime = 3f;
        knockBackVelocity = 3;
        staggerVelocity = 2;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        landImmobalizedTime = .5f;
        minTimeToLandTime = .5f;
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
                minTimeToLandTimer -= Time.deltaTime;
                if (isGrounded && minTimeToLandTimer <= 0)
                    StartCoroutine(SetImmobilizeFor(landImmobalizedTime));
                break;
            case State.Immobilized:
                break;
            case State.Falling:
                minTimeToLandTimer -= Time.deltaTime;
                if (isGrounded && minTimeToLandTimer <= 0)
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
        minTimeToLandTimer = minTimeToLandTime;
        retreatTimer = Random.Range(minRetreatTime, maxRetreatTime);
    }

    void StartShootingProjectile()
    {
        SetAttack1();
    }
    void ShootProjectileEvent()
    {
        Instantiate(projectilePrefab).GetComponent<Projectile>()
            .Set(projectileSpawnPoint.position, new Vector2(coll.bounds.center.x < target.position.x ? 1 : -1, 0),
            "EnemyWeapon", target.position, projectileSpeed, projectileDamage);
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
        }
    }
    protected void GetHit(bool isRight, float damage, bool doKnockback)
    {
        health -= damage;
        print(name + " Health: " + health);
        if (health <= 0)
        {
            if (state != State.Dead)
                SetDead(isRight);
        }
        else
        {
            if (!doKnockback)
                SetStaggered(isRight);
            else
                SetKnockedBack(isRight);
        }
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
