using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Ghoul : AI_Base
{
    public float attackDamage;
    public float jumpDamage;

    public AudioClip attackSound;
    public AudioClip getHitSound;
    private AI_Soundmanager soundManager;

    private float maxAggroRange;
    private LayerMask terrainMask;
    private LayerMask playerMask;
    private float attackRange;
    private float maxJumpRange;
    private float minJumpRange;
    private float jumpVelocity;
    private float minYJumpDist;
    private float maxYJumpDist;
    private float attackImmobalizeTime = .5f;
    private float landImmobalizedTime = .5f;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private bool isNextDoubleAttack;
    private bool isGrounded;
    private float minJumpInterval = .5f;
    private float minJumpTimer;

    void Awake()
    {
        soundManager = GetComponent<AI_Soundmanager>();
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 5;
        aggroRange = 4;
        maxAggroRange = 10;
        knockBackVelocity = 5;
        staggerVelocity = 2;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        attackRange = 1f;
        maxJumpRange = 7;
        minJumpRange = 3;
        jumpVelocity = 8;
        minYJumpDist = 2;
        attackImmobalizeTime = .5f;
        landImmobalizedTime = .5f;
        //-----------------------------------------------

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        SetIdle();
        maxYJumpDist = jumpVelocity * .5f;

        stateAfterAttackCall = () => SetAggro();
        stateAfterKnockbackCall = () => SetAwakening();
        stateAfterStaggeredCall = () => SetAggro();
        stateAfterAwake = () => SetAggro();
    }
    void Update()
    {
        isGrounded = IsGrounded(terrainMask);
        switch (state)
        {
            case State.Aggro:
                bool inLineWithPlayer = RaycastToPlayer(isDirRight, maxAggroRange, playerTag, playerMask, terrainMask) ||
                                        RaycastToPlayer(!isDirRight, maxAggroRange, playerTag, playerMask, terrainMask);
                float dist = Vector2.Distance(transform.position, target.position);
                bool goundInFrontExists = DoesGroundForwardExists(isDirRight, yRayLength, terrainMask);
                float yDiff = target.position.y - transform.position.y;

                if (dist + .1f > maxAggroRange)
                {
                    SetIdle();
                    break;
                }
                if (!goundInFrontExists && isGrounded && yDiff >= -coll.bounds.extents.y)
                {
                    AttackJump();
                }
                else if (isGrounded && RaycastSideways_OR(isDirRight, 3, xRayLength, terrainMask))
                {
                    AttackJump();
                }
                else if (inLineWithPlayer) // player on the same level as enemy
                {
                    if (target.position.x < transform.position.x)
                    {
                        if (isDirRight)
                            ChangeDirection(false);
                    }
                    else
                    {
                        if (!isDirRight)
                            ChangeDirection(true);
                    }

                    if (Vector2.Distance(target.position, transform.position) < attackRange)
                        AttackSingle();
                }
                else // player is either higher or lower
                {
                    if (isGrounded && yDiff >= minYJumpDist && yDiff < maxYJumpDist && dist >= minJumpRange && dist < maxJumpRange) // player is higher - perform jump to it
                        AttackJump();
                    else if (dist < attackRange)
                        AttackSingle();
                }
                break;
            case State.Attacking:
                break;
            case State.KnockedBack:
                if (isGrounded && fullyKnockedDown && state != State.Dead)
                    EndKnockedBack();
                break;
            case State.Idle:
                if (Vector2.Distance(transform.position, target.position) < aggroRange)
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
                minJumpTimer -= Time.deltaTime;
                if (isGrounded && minJumpTimer <= 0)
                    StartCoroutine(SetImmobilizeFor(landImmobalizedTime));
                break;
            case State.Immobilized:
                break;
            case State.Falling:
                minJumpTimer -= Time.deltaTime;
                if (isGrounded && minJumpTimer <= 0)
                    SetAggro();
                break;
        }
        animator.SetBool("Is Grounded", isGrounded);
        animator.SetFloat("Vertical Velocity", rb.velocity.y);
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }
    void AttackJump()
    {
        Vector2 distToTarget = target.position - transform.position;
        damageContainer.SetDamageCall(() => jumpDamage);
        Vector2 jumpVel = new Vector2(target.position.x > transform.position.x ? 1 : -1, distToTarget.y > (coll.bounds.extents.y / 2f) ? 3 : 1).normalized;
        float velMult = Mathf.Clamp(distToTarget.magnitude, 2, jumpVelocity);

        SetJumping(jumpVel * jumpVelocity);
        ChangeDirection(jumpVel.x > 0);
        minJumpTimer = minJumpInterval;
    }
    void AttackSingle()
    {
        damageContainer.SetDamageCall(() => attackDamage);
        SetAttack1();
    }

    void EndAttack()    // ovverides AI_Base.EndAttack() on animation events
    {
        StartCoroutine(SetImmobilizeFor(attackImmobalizeTime));
    }
    IEnumerator SetImmobilizeFor(float time)
    {
        ChangeDirection(transform.position.x < target.position.x);
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

                    rb.velocity = new Vector2(direction.x * movVelocity, rb.velocity.y);
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
                {
                    float yDiff = target.position.y - transform.position.y;
                    bool islookingAtTarget = (transform.position.x > target.position.x && !isDirRight) ||
                                             (transform.position.x < target.position.x && isDirRight);
                    bool isTargetInLineOfSight = Mathf.Abs(yDiff) < 0.5f;

                    if (isTargetInLineOfSight && isWallBlockingTopOfCollider(isDirRight, terrainMask) && isWallBlockingBottomOfCollider(isDirRight, terrainMask))
                    {
                        SetFalling();
                    }
                    else if (islookingAtTarget && !RaycastSideways_OR(isDirRight, 3, xRayLength, terrainMask))
                    {
                        Vector2 direction;
                        if (isDirRight)
                            direction = Vector2.right;
                        else
                            direction = Vector2.left;
                        rb.velocity = new Vector2(direction.x * movVelocity, rb.velocity.y);
                    }
                    break;
                }
            case State.Falling:
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
                SetDead(isRight, this.GetType());
            target.GetComponent<Player>().AddEnemyKilldedToCount(this.GetType());
        }
        else
        {
            if (!doKnockback)
                SetStaggered(isRight);
            else
                SetKnockedBack(isRight);
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
            GetHit(transform.position.x < other.transform.position.x,
                dc.GetDamage(), dc.doKnockback());
        }
    }

    private void PlayAttackEfect()
    {
        soundManager.PlayOneShot(attackSound);
    }
}
