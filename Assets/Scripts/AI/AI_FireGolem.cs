using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_FireGolem : AI_Base
{
    public LayerMask terrainMask;
    public LayerMask playerMask;
    public float attackRange;
    public float attackDamage;
    [Range(0, 1)] public float doubleAttackChance;
    public float doubleAttackRange;
    public float doubleAttackDamage;
    public float maxJumpRange, minJumpRange;
    public float jumpDamage;
    public float jumpVelocity;
    public float minYJumpDist;
    public float maxYDiff;
    private float maxYJumpDist;
    public float attackImmobalizeTime = .5f;
    public float landImmobalizedTime = .5f;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private bool isNextDoubleAttack;
    private bool isGrounded;
    private float minJumpInterval = .5f;
    private float minJumpTimer;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        damageContainer.SetDamageCall(() => attackDamage);
        health = maxHealth;
        isNextDoubleAttack = Random.value < doubleAttackChance;
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
                bool inLineWithPlayer = RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask, Color.magenta) ||
                                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask, Color.magenta);
                float dist = Vector2.Distance(transform.position, target.position);
                bool goundInFrontExists = DoesGroundForwardExists(isDirRight, yRayLength, terrainMask, Color.blue);
                float yDiff = target.position.y - transform.position.y;

                if (!goundInFrontExists && isGrounded && yDiff >= -coll.bounds.extents.y)
                    AttackJump();
                else if (isGrounded && RaycastSideways_OR(isDirRight, 3, xRayLength, terrainMask, Color.red))
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
                    if (RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask, Color.blue) ||
                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask, Color.blue))
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
                    if (islookingAtTarget && !RaycastSideways_OR(isDirRight, 3, xRayLength, terrainMask, Color.red))
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
            GetHit(transform.position.x < other.transform.position.x,
                dc.GetDamage(), dc.doKnockback());
        }
    }
}
