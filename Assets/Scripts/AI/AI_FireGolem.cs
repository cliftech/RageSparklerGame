using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_FireGolem : AI_Base
{
    public LayerMask changeDirMask;
    public LayerMask playerMask;
    public LayerMask enemyMask;
    public LayerMask groundMask;
    public float attackRange;
    public float attackDamage;
    [Range(0, 1)] public float doubleAttackChance;
    public float doubleAttackRange;
    public float doubleAttackDamage;
    public float maxJumpRange, minJumpRange;
    public float jumpDamage;
    public float jumpVelocity;
    public float minYJumpDist;
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
    }
    void Update()
    {
        isGrounded = IsGrounded(groundMask);
        switch (state)
        {
            case State.Patrol:
                break;
            case State.Aggro:
                bool inLineWithPlayer = RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, changeDirMask, Color.magenta) ||
                                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, changeDirMask, Color.magenta);

                // player on the same level as enemy
                if (inLineWithPlayer)
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
                    float dist = Vector2.Distance(transform.position, target.position);
                    float yDiff = target.position.y - transform.position.y;
                    if (yDiff >= minYJumpDist && yDiff < maxYJumpDist && dist >= minJumpRange && dist < maxJumpRange) // player is higher - perform jump to it
                        AttackJump(dist);
                    else if (isNextDoubleAttack && dist < doubleAttackRange)
                        AttackDouble();
                    else if (!isNextDoubleAttack && dist < attackRange)
                        AttackSingle();
                }
                break;
            case State.Attacking:
                break;
            case State.KnockedBack:
                knockBackTimer -= Time.deltaTime;
                if (knockBackTimer <= 0)
                {
                    EndKnockedBack();
                    SetFalling();
                }
                break;
            case State.Idle:
                if (Vector2.Distance(transform.position, target.position) < aggroRange)
                {
                    if (RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, changeDirMask, Color.blue) ||
                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, changeDirMask, Color.blue))
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
            case State.Running:
                break;
            case State.Jumping:
                if (isGrounded)
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
    void AttackJump(float distToTarget)
    {
        damageContainer.SetDamageCall(() => jumpDamage);
        Vector2 jumpVel = new Vector2(target.position.x > transform.position.x ? 1 : -1, 1).normalized;
        float velMult = Mathf.Clamp(distToTarget, 2, jumpVelocity);
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
            case State.Patrol:
                break;
            case State.Aggro:
                {
                    Vector2 direction;
                    if (isDirRight)
                        direction = Vector2.right;
                    else
                        direction = Vector2.left;

                    if (RaycastSideways_OR(isDirRight, 5, xRayLength, enemyMask, Color.red))
                        rb.velocity = new Vector2(0, rb.velocity.y);
                    else
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
            case State.Running:
                break;
            case State.Jumping:
                break;
            case State.Immobilized:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Falling:
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerWeaponTag) && state != State.KnockedBack && state != State.Dead)
        {
            GetHit(transform.position.x < other.transform.position.x, other.GetComponentInParent<DamageContainer>().GetDamage());
        }
    }
}
