using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_FireGolem : AI_Base
{
    public float attackDamage;
    [Range(0, 1)] public float doubleAttackChance;
    public float doubleAttackDamage;
    public float jumpDamage;

    private LayerMask terrainMask;
    private LayerMask playerMask;
    private float attackRange;
    private float doubleAttackRange;
    private float maxJumpRange;
    private float minJumpRange;
    private float jumpVelocity;
    private float minYJumpDist;
    private float maxYDiff;
    private float maxYJumpDist;
    private float landImmobalizedTime;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private bool isNextDoubleAttack;
    private bool isGrounded;
    private float minJumpInterval;
    private float minJumpTimer;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 3;
        aggroRange = 5;
        attackRange = 1.5f;
        doubleAttackRange = 1.5f;
        maxJumpRange = 10f;
        minJumpRange = 2.5f;
        jumpVelocity = 10f;
        minYJumpDist = 1;
        maxYDiff = 5;
        knockBackVelocity = 1;
        staggerVelocity = 0.5f;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        landImmobalizedTime = .5f;
        minJumpInterval = .5f;
        maxYJumpDist = jumpVelocity * .5f;
        //-----------------------------------------------

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        isNextDoubleAttack = Random.value < doubleAttackChance;
        SetIdle();

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
                bool inLineWithPlayer = RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask) ||
                                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask);
                float dist = Vector2.Distance(transform.position, target.position);
                bool goundInFrontExists = DoesGroundForwardExists(isDirRight, yRayLength, terrainMask);
                float yDiff = target.position.y - transform.position.y;

                if (!goundInFrontExists && isGrounded && yDiff >= -coll.bounds.extents.y)
                    AttackJump();
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

                    if (!isNextDoubleAttack && Vector2.Distance(target.position, transform.position) < attackRange)
                        AttackSingle();
                    else if (isNextDoubleAttack && Vector2.Distance(target.position, transform.position) < doubleAttackRange)
                        AttackDouble();
                }
                else // player is either higher or lower
                {
                    if (yDiff > maxYDiff)
                        SetIdle();
                    else
                    {
                        if (isGrounded && yDiff >= minYJumpDist && yDiff < maxYJumpDist && dist >= minJumpRange && dist < maxJumpRange) // player is higher - perform jump to it
                            AttackJump();
                        else if (isNextDoubleAttack && dist < doubleAttackRange)
                            AttackDouble();
                        else if (!isNextDoubleAttack && dist < attackRange)
                            AttackSingle();
                    }
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
                        aggroRange *= 5;
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
                {
                    damageContainer.SetDamageCall(() => touchDamage);
                    SetAggro();
                }
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
        isNextDoubleAttack = Random.value < doubleAttackChance;
        minJumpTimer = minJumpInterval;
    }
    void AttackSingle()
    {
        damageContainer.SetDamageCall(() => attackDamage);
        SetAttack1();
        isNextDoubleAttack = Random.value < doubleAttackChance;
    }
    void AttackDouble()
    {
        damageContainer.SetDamageCall(() => doubleAttackDamage);
        SetAttack2();
        isNextDoubleAttack = Random.value < doubleAttackChance;
    }
    void EndAttack()    // ovverides AI_Base.EndAttack() on animation events
    {
        damageContainer.SetDamageCall(() => touchDamage);
        StartCoroutine(SetImmobilizeFor(landImmobalizedTime));
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

                    if (isWallBlockingTopOfCollider(isDirRight, terrainMask) && isWallBlockingBottomOfCollider(isDirRight, terrainMask))
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
        cameraController.Shake(damage);
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
}
