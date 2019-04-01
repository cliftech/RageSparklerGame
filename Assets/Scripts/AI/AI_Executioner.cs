using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Executioner : AI_Base
{
    public GameObject shockwavePrefab;
    public float attackDamage;
    public float shockwaveDamage;

    private LayerMask terrainMask;
    private LayerMask playerMask;
    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";

    private float attackRange;
    private float attackImmobilizedTime;

    private float maxJumpRange;
    private float minJumpRange;
    private bool isGrounded;
    private float minJumpInterval;
    private float minJumpTimer;
    private float jumpWindUpTime;
    private float landImmobalizedTime;

    private float chargeWindUpTime;
    private float maxChargeRange;
    private float minChargeRange;
    private float maxChargeTime;
    private float chargeTimer;
    private float wallStunTime;

    private float staggerFalloff;
    private float staggerCounter;
    private int maxStaggerCount;

    enum AttackType { NormalAttack, ChargeAttack, JumpAttack }
    AttackType nextAttack;
    private float tendencyToNormalAttack;
    private float tendencyToChargeAttack;
    private float tendencyToJumpAttack;


    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 5;
        aggroRange = 10;
        attackRange = 2;
        maxJumpRange = 10f;
        minJumpRange = 2.5f;
        knockBackVelocity = 1;
        staggerVelocity = 0.5f;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        landImmobalizedTime = 2f;
        minJumpInterval = .5f;

        jumpWindUpTime = 0.75f;
        attackImmobilizedTime = 0.5f;

        maxChargeRange = 5;
        minChargeRange = attackRange * 1.2f;
        chargeWindUpTime = .75f;
        maxChargeTime = 3f;
        wallStunTime = 1.5f;

        staggerFalloff = 1;
        maxStaggerCount = 3;

        //these have to be in ascending order, if not change this.DetermineNextAttack method
        // these have to add up to one
        tendencyToNormalAttack = 0.1f;
        tendencyToJumpAttack = 0.3f;
        tendencyToChargeAttack = 0.6f;
        //-----------------------------------------------

        tendencyToJumpAttack += tendencyToNormalAttack;
        tendencyToChargeAttack += tendencyToJumpAttack;

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

        if (staggerCounter > 0)
            staggerCounter -= Time.deltaTime * staggerFalloff;

        switch (state)
        {
            case State.Aggro:
                {
                    float dist = Vector2.Distance(coll.bounds.center, target.position);
                    bool goundInFrontExists = DoesGroundForwardExists(isDirRight, yRayLength, terrainMask);
                    float yDiff = target.position.y - coll.bounds.center.y;

                    if (!goundInFrontExists && isGrounded && yDiff >= -coll.bounds.extents.y)
                    {
                        AttackJump();
                        break;
                    }

                    if (yDiff < 1) // player on the same level as enemy
                    {
                        if (dist < maxChargeRange && dist > minChargeRange)
                            ChargeStart();
                        else if (dist < maxJumpRange && dist > minJumpRange)
                            AttackJump();
                        else if (dist < attackRange)
                            AttackSingle();
                    }
                    else if (dist < maxJumpRange && dist > minJumpRange)
                        AttackJump();
                    else if (yDiff > 8)
                        SetIdle();

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
                    Land();
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
            case State.Charging:
                {
                    bool wallInFront = RaycastSideways_OR(isDirRight, 5, xRayLength, terrainMask);
                    float dist = Vector2.Distance(coll.bounds.center, target.position);

                    chargeTimer -= Time.deltaTime;

                    if (dist < attackRange)
                    {
                        EndCharge(true, false);
                    }
                    else if (chargeTimer <= 0)
                    {
                        EndCharge(false, false);
                    }
                    else if (wallInFront)
                    {
                        EndCharge(false, true);
                    }
                }
                break;
        }
        animator.SetBool("Is Grounded", isGrounded);
        animator.SetFloat("Vertical Velocity", rb.velocity.y);
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }
    void DetermineNextAttack()
    {
        float r = Random.value;
        if (r < tendencyToNormalAttack)
            nextAttack = AttackType.NormalAttack;
        else if (r >= tendencyToNormalAttack && r < tendencyToJumpAttack)
            nextAttack = AttackType.JumpAttack;
        else
            nextAttack = AttackType.ChargeAttack;
    }

    void AttackJump()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        animator.SetTrigger("Jump");
        StartCoroutine(JumpWindUp(jumpWindUpTime));
        SetImmobilized();
    }
    IEnumerator JumpWindUp(float time)
    {
        yield return new WaitForSecondsRealtime(time);

        Vector2 distToTarget = target.position - transform.position;
        Vector2 jumpVel = new Vector2(target.position.x > transform.position.x ? 1 : -1, distToTarget.y > (coll.bounds.extents.y / 2f) ? 3 : 1).normalized;
        float velMult = Mathf.Clamp(distToTarget.magnitude, 2, 10);
        animator.SetTrigger("Jump");
        state = State.Jumping;
        rb.velocity = jumpVel * 10;
        minJumpTimer = minJumpInterval;
        DetermineNextAttack();
    }
    void Land()
    {
        Vector2 shockwaveOrigin = coll.bounds.center;
        shockwaveOrigin.y = coll.bounds.min.y;
        Instantiate(shockwavePrefab, transform.parent).GetComponent<Shockwave>().Set(shockwaveOrigin, 3f, Vector2.right, shockwaveDamage);
        Instantiate(shockwavePrefab, transform.parent).GetComponent<Shockwave>().Set(shockwaveOrigin, 3f, Vector2.left, shockwaveDamage);

        StartCoroutine(SetImmobilizeFor(landImmobalizedTime));
    }
    void AttackSingle()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        damageContainer.SetDamageCall(() => attackDamage);
        SetAttack1();
        DetermineNextAttack();
    }
    void ChargeStart()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        animator.SetTrigger("Charge");
        StartCoroutine(ChargeWindUp(chargeWindUpTime));
        SetImmobilized();
    }
    IEnumerator ChargeWindUp(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        chargeTimer = maxChargeTime;
        animator.SetBool("Charging", true);
        state = State.Charging;
        DetermineNextAttack();
    }
    void EndCharge(bool attack, bool stun)
    {
        if (attack)
            AttackSingle();
        else if (stun)
            StartCoroutine(Stunned(wallStunTime, () => SetAggro()));
        else
            SetAggro();
        animator.SetBool("Charging", false);
    }
    IEnumerator Stunned(float time, System.Action actionAfterStun)
    {
        state = State.Stunned;
        animator.SetTrigger("GetHit");
        yield return new WaitForSecondsRealtime(time);
        actionAfterStun.Invoke();
        animator.SetBool("Stunned", false);
    }
    void EndAttack()    // ovverides AI_Base.EndAttack() on animation events
    {
        damageContainer.SetDamageCall(() => touchDamage);
        StartCoroutine(SetImmobilizeFor(attackImmobilizedTime));
    }
    new void EndStagger() // ovverides AI_Base.EndStagger() on animation events
    {
        if (state != State.Stunned)
            stateAfterStaggeredCall();
        else
            animator.SetBool("Stunned", true);
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
                    switch (nextAttack)
                    {
                        case AttackType.NormalAttack:
                            {
                                bool b = target.position.x < coll.bounds.center.x;
                                if (isDirRight != b)
                                    ChangeDirection(!b);
                            }
                            break;
                        case AttackType.ChargeAttack:
                            {
                                bool b = target.position.x < coll.bounds.center.x;
                                if (isDirRight != b)
                                    ChangeDirection(!b);
                            }
                            break;
                        case AttackType.JumpAttack:
                            {
                                bool b = target.position.x < coll.bounds.center.x;
                                if (isDirRight != b)
                                    ChangeDirection(!b);
                            }
                            break;
                    }

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
            case State.Charging:
                {
                    Vector2 direction;
                    if (isDirRight)
                        direction = Vector2.right;
                    else
                        direction = Vector2.left;

                    rb.velocity = new Vector2(direction.x * movVelocity * 1.5f, rb.velocity.y);
                }
                break;
            case State.Stunned:
                {
                    if (isGrounded)
                        rb.velocity = Vector2.zero;
                }
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
            bool stagger = false;
            if (doKnockback || staggerCounter + 1 <= maxStaggerCount)
                stagger = true;

            if (stagger)
            {
                staggerCounter += 1;
                SetStaggered(isRight);
            }
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
