using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Slug : MonoBehaviour
{
    private Rigidbody2D rb;
    new private SpriteRenderer renderer;
    private CapsuleCollider2D coll;
    private Animator animator;
    private Transform target;
    private DamageContainer damageContainer;
    private Player player;

    public enum State { Patrol, Aggro, Attacking, KnockedBack };
    private State state;

    private bool isDirRight;
    private float xRayLength;
    private Vector2 originalScale;
    private float health;
    private float knockBackTimer;

    public LayerMask wallMask;
    public string playerWeaponTag;
    public float movVelocity;
    public float aggroRange;
    public float attackRange;
    public float attackDamage;
    public float maxHealth;
    public float knockBackVelocity;
    public float knockBackTime;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        damageContainer = GetComponent<DamageContainer>();
        target = FindObjectOfType<Player>().transform;
        player = FindObjectOfType<Player>();
    }
    void Start()
    {
        xRayLength = coll.size.x / 2 + 0.05f;
        originalScale = transform.localScale;
        damageContainer.SetDamageCall(() => attackDamage);
        health = maxHealth;
        SetPatrol();
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.Patrol)
        {
            if (RaycastSideways_OR(isDirRight, 5, Color.red, wallMask))
            {
                ChangeDirection(!isDirRight);
            }
            if (Vector2.Distance(target.position, transform.position) < aggroRange)
            {
                SetAggro();
            }
        }
        else if(state == State.Aggro)
        {
            if(target.position.x < transform.position.x )
            {
                if(isDirRight)
                {
                    ChangeDirection(false);
                }
            }
            else
            {
                if(!isDirRight)
                {
                    ChangeDirection(true);
                }
            }
            if (Vector2.Distance(target.position, transform.position) < attackRange)
            {
                SetAttack();
            }
        }
        else if(state == State.KnockedBack)
        {
            knockBackTimer -= Time.deltaTime;
            if(knockBackTimer <= 0)
            {
                EndKnockedBack();
            }
        }
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }
    void FixedUpdate()
    {
        if (state == State.Patrol)
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
        }
        else if (state == State.Aggro)
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
        }
        else if (state == State.Attacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else if (state == State.KnockedBack)
        {
        }
    }
    void SetPatrol()
    {
        state = State.Patrol;
    }
    void SetAggro()
    {
        state = State.Aggro;
    }
    void SetAttack()
    {
        state = State.Attacking;
        animator.SetTrigger("Attack");
    }
    void EndAttack()
    {
        SetAggro();
    }
    void SetKnockedBack()
    {
        state = State.KnockedBack;
        knockBackTimer = knockBackTime;
    }
    void EndKnockedBack()
    {
        SetAggro();
        knockBackTimer = 0;
    }
    void ChangeDirection(bool toRight)
    {
        if (toRight)
        {
            transform.localScale = new Vector2(-originalScale.x, transform.localScale.y);
        }
        else
        {
            transform.localScale = new Vector2(originalScale.x, transform.localScale.y);
        }
        isDirRight = toRight;
    }
    void GetHit(bool isRight, float damage)
    {
        health -= damage;
        KnockBack(isRight, damage);
    }
    public void KnockBack(bool knockbackToLeftSide, float forceMult)
    {
        Vector2 forceDir = (knockbackToLeftSide ? new Vector2(-1, .5f) : new Vector2(1, .5f)).normalized;
        rb.velocity = forceDir * knockBackVelocity * Mathf.Clamp(forceMult / 100, 1, 1.5f);
        SetKnockedBack();
        ChangeDirection(knockbackToLeftSide);

        animator.SetTrigger("GetHit");
    }
    bool RaycastSideways_OR(bool isRight, int perpRayCount, Color debugColor, LayerMask mask)
    {
        int direction;
        if (isRight)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }
        Vector2 origin = coll.bounds.center;
        if (perpRayCount == 1)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRayLength, debugColor, 0.075f);
            return Physics2D.Raycast(origin, Vector2.right * direction, xRayLength, mask);
        }
        float yOffset = coll.size.y / perpRayCount;
        origin.y -= yOffset * perpRayCount / 2;
        for (int i = 0; i < perpRayCount + 1; i++)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRayLength, debugColor, 0.075f);
            if (Physics2D.Raycast(origin, Vector2.right * direction, xRayLength, mask))
                return true;
            origin.y += yOffset;
        }
        return false;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag(playerWeaponTag))
        {
            GetHit(transform.position.x < other.transform.position.x, player.GetDamage());
        }
    }

}
