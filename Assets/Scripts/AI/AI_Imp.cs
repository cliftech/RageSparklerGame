using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Imp : AI_Base
{
    public float attackDamage;

    private LayerMask terrainMask;
    private LayerMask playerMask;
    private float attackRange;
    private float loseAggroRange;
    private float minDistanceToTarget;
    private float attackCooldown;
    private bool canAttack = true;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    private float projectileSpeed;
    private float maxDistanceFromGround;

    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";
    private string trapTag = "Trap";

    void Awake()
    {
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 3;
        aggroRange = 7;
        attackRange = 7;
        loseAggroRange = 9;
        staggerVelocity = 1;
        minDistanceToTarget = 2;
        attackCooldown = 2;
        projectileSpeed = 6;
        maxDistanceFromGround = 3;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        //-----------------------------------------------

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        SetPatrol();

        stateAfterKnockbackCall = () => SetAwakening();
        stateAfterStaggeredCall = () => SetAggro();
        stateAfterAwake = () => SetAggro();
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, target.position);
        switch (state)
        {
            case State.Patrol:
                if (distance < aggroRange)
                    SetAggro();
                break;
            case State.Aggro:
                if (distance > loseAggroRange)
                    SetPatrol();
                else if(canAttack && distance < attackRange)
                    Attack();
                break;
            case State.Attacking:
                break;
            case State.Dead:
                break;
        }

        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                rb.velocity = Vector2.zero;
                break;
            case State.Aggro:
                bool b = target.position.x < coll.bounds.center.x;
                if (isDirRight == b)
                    ChangeDirection(!b);
                float distanceToGround;
                RaycastHit2D hit = Physics2D.Raycast(coll.bounds.center, Vector2.down, aggroRange, terrainMask);
                if(hit)                {
                    distanceToGround = Vector2.Distance(coll.bounds.center, hit.point);                }
                else                {
                    distanceToGround = aggroRange;                }
                float distance = Vector2.Distance(transform.position, target.position);
                if(distance > minDistanceToTarget && distance < attackRange)
                {
                    Vector2 direction = Vector2.zero;
                    if(distanceToGround > maxDistanceFromGround)                    {
                        direction.y = -1;                    }
                    rb.velocity = direction * movVelocity;
                }
                else if(distance > attackRange)
                {
                    Vector2 direction = (target.position - transform.position).normalized;
                    rb.velocity = direction * movVelocity;
                }
                else if(distance < minDistanceToTarget)
                {
                    Vector2 direction = (target.position - transform.position).normalized;
                    rb.velocity = -direction * movVelocity;
                }
                else
                    rb.velocity = Vector2.zero;
                break;
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

    void Attack()
    {
        ChangeDirection(transform.position.x < target.position.x);
        SetAttack1();
    }

    void SummonProjectile()
    {
        Instantiate(projectilePrefab, transform.parent).GetComponent<Projectile>().Set(projectileSpawnPoint.position, 
            new Vector2(coll.bounds.center.x < target.position.x?1:-1,0), "EnemyWeapon", target.position, projectileSpeed, attackDamage);
    }

    void EndAttack()
    {
        SetAggro();
        StartCoroutine(DelayAttackCooldown());
    }

    IEnumerator DelayAttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSecondsRealtime(attackCooldown);
        canAttack = true;
    }

    protected void GetHit(bool isRight, float damage, bool doKnockback)
    {
        health -= damage;
        if (health <= 0)
            if (state != State.Dead)
                SetDead(isRight, 5f);
        else
            SetStaggered(isRight);
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(trapTag) && state != State.Dead)
            GetHit(rb.velocity.x > 0, maxHealth, false);
    }
}
