using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Slug : AI_Base
{
    public LayerMask terrainMask;
    public LayerMask playerMask;
    public float attackRange;
    public float attackDamage;
    [Range(0, 1)] public float vomitAttackChance;
    public float vomitAttackRange;
    public float vomitAttackDamage;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private bool isNextAttackVomit;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        damageContainer.SetDamageCall(() => attackDamage);
        health = maxHealth;
        isNextAttackVomit = Random.value < vomitAttackChance;
        SetPatrol();

        stateAfterAttackCall = () => SetAggro();
        stateAfterKnockbackCall = () => SetAwakening();
        stateAfterStaggeredCall = () => SetAggro();
        stateAfterAwake = () => SetAggro();
    }
    void Update()
    {
        switch (state)
        {
            case State.Patrol:
                if (!DoesGroundForwardExists(isDirRight, yRayLength, terrainMask, Color.blue) ||
                    RaycastSideways_OR(isDirRight, 5, xRayLength, terrainMask, Color.red))
                {
                    ChangeDirection(!isDirRight);
                }
                if (Vector2.Distance(transform.position, target.position) < aggroRange)
                {
                    if (RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask, Color.blue))
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
                if (!RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask, Color.blue))
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
                    if (!IsGrounded(terrainMask) || !DoesGroundForwardExists(isDirRight, yRayLength, terrainMask, Color.blue))
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
    protected void GetHit(bool isRight, float damage, bool doKnockback)
    {
        health -= damage;
        print(name + " Health: " + health);
        if (health <= 0)
        {
            if (state != State.Dead)
                SetDead();
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
