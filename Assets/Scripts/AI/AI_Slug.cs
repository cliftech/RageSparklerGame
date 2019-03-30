using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Slug : AI_Base
{

    public float attackDamage;
    [Range(0, 1)] public float vomitAttackChance;
    public float vomitAttackDamage;

    private LayerMask terrainMask;
    private LayerMask playerMask;
    private float attackRange;
    private float vomitAttackRange;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private string trapTag = "Trap";
    private bool isNextAttackVomit;
    private bool isgrounded;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 1.5f;
        aggroRange = 5;
        attackRange = .85f;
        vomitAttackRange = 1f;
        knockBackVelocity = 1;
        staggerVelocity = 0.5f;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        //-----------------------------------------------

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        isNextAttackVomit = Random.value < vomitAttackChance;
        SetPatrol();

        stateAfterAttackCall = () => AttackEnded();
        stateAfterKnockbackCall = () => SetAwakening();
        stateAfterStaggeredCall = () => SetAggro();
        stateAfterAwake = () => SetAggro();
    }

    void Update()
    {
        isgrounded = IsGrounded(terrainMask);

        switch (state)
        {
            case State.Patrol:
                if ((!DoesGroundForwardExists(isDirRight, yRayLength, terrainMask) ||
                    RaycastSideways_OR(isDirRight, 5, xRayLength, terrainMask)) && isgrounded)
                {
                    ChangeDirection(!isDirRight);
                }
                if (Vector2.Distance(transform.position, target.position) < aggroRange)
                {
                    if (RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask))
                    {
                        SetAggro();
                    }
                }
                break;
            case State.Aggro:
                if (target.position.x < transform.position.x)
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
                if (!isNextAttackVomit)
                {
                    if (Vector2.Distance(target.position, transform.position) < attackRange)
                    {
                        damageContainer.SetDamageCall(() => attackDamage);
                        SetAttack1();
                        isNextAttackVomit = Random.value < vomitAttackChance;
                    }
                }
                else
                {
                    if (Vector2.Distance(target.position, transform.position) < vomitAttackRange)
                    {
                        damageContainer.SetDamageCall(() => vomitAttackDamage);
                        SetAttack2();
                        isNextAttackVomit = Random.value < vomitAttackChance;
                    }
                }
                if (!RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask))
                {
                    SetPatrol();
                }
                break;
            case State.Attacking:
                break;
            case State.KnockedBack:
                if (IsGrounded(terrainMask) && fullyKnockedDown && state != State.Dead)
                    EndKnockedBack();
                break;
            case State.Dead:
                break;
            case State.Falling:
                if (IsGrounded(terrainMask))
                    SetAggro();
                break;
        }

        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                {
                    Vector2 direction;
                    if (isDirRight)
                    {
                        direction = Vector2.right;
                    }
                    else
                    {
                        direction = Vector2.left;
                    }
                    rb.velocity = new Vector2(direction.x * movVelocity, rb.velocity.y);
                    break;
                }
            case State.Aggro:
                {
                    Vector2 direction;
                    if (isDirRight)
                    {
                        direction = Vector2.right;
                    }
                    else
                    {
                        direction = Vector2.left;
                    }
                    if (!IsGrounded(terrainMask) || !DoesGroundForwardExists(isDirRight, yRayLength, terrainMask))
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                    }
                    else
                    {
                        rb.velocity = new Vector2(direction.x * movVelocity, rb.velocity.y);
                    }
                    break;
                }
            case State.Attacking:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.KnockedBack:
                break;
            case State.Idle:
                break;
            case State.Dead:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Awakening:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Running:
                break;
        }
    }
    void AttackEnded()
    {
        damageContainer.SetDamageCall(() => touchDamage);
        SetAggro();
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(trapTag) && state != State.Dead)
        {
            GetHit(rb.velocity.x > 0, maxHealth, false);
        }
    }
}
