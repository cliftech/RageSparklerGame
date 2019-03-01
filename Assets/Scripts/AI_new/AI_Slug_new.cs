using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Slug_new : AI_Base
{
    public LayerMask changeDirMask;
    public LayerMask playerMask;
    public LayerMask enemyMask;
    public string playerTag;
    public string playerWeaponTag;
    public float attackRange;
    public float attackDamage;
    [Range(0, 1)] public float vomitAttackChance;
    public float vomitAttackRange;
    public float vomitAttackDamage;

    private bool isNextAttackVomit;

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        xRayLength = 0.05f;
        originalScale = transform.localScale;
        damageContainer.SetDamageCall(() => attackDamage);
        health = maxHealth;
        aliveColor = renderer.color;
        isNextAttackVomit = Random.value < vomitAttackChance;
        SetPatrol();
    }
    void Update()
    {
        switch (state)
        {
            case State.Patrol:
                if (RaycastSideways_OR(isDirRight, 5, xRayLength, changeDirMask, Color.red) ||
                    RaycastSideways_OR(isDirRight, 5, xRayLength, enemyMask, Color.red))
                {
                    ChangeDirection(!isDirRight);
                }
                if (Vector2.Distance(transform.position, target.position) < aggroRange)
                {
                    if (RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, changeDirMask, Color.blue))
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
                if (!RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, changeDirMask, Color.blue))
                {
                    SetPatrol();
                }
                break;
            case State.Attacking:
                break;
            case State.KnockedBack:
                knockBackTimer -= Time.deltaTime;
                if (knockBackTimer <= 0)
                {
                    EndKnockedBack();
                }
                break;
            case State.Idle:
                break;
            case State.Dead:
                break;
            case State.Awakening:
                break;
            case State.Running:
                break;
        }

        if (state != State.Dead && health <= 0)
        {
            SetDead();
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
                    if (RaycastSideways_OR(isDirRight, 5, xRayLength, enemyMask, Color.red))
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerWeaponTag) && state != State.KnockedBack && state != State.Dead)
        {
            GetHit(transform.position.x < other.transform.position.x, other.GetComponentInParent<DamageContainer>().GetDamage());
        }
    }
}
