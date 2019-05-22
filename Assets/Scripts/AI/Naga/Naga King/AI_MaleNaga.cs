using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_MaleNaga : AI_Base
{
    private Naga_Manager nagaManager;
    private AI_Soundmanager sound;
    private LayerMask terrainMask;
    private LayerMask playerMask;

    public AudioClip simpleAttackSound;
    public AudioClip whirlwindAttackSound;
    public AudioClip getHitSound;
    public GameObject whirlwindEffectPrefab;
    public GameObject enragedPrefab;

    public bool startFacingLeft;

    public float simpleAttackDamage;
    private float simpleAttackRange;
    private float cantAttackTimeAfterSimpleAttack;
    private bool canAttackSimple = true;

    public float whirlwindAttackDamage;
    private float whirlwindWindupTime;
    private float whirlwindCastTime;
    private float immobilizedTimeAfterWhirlwind;
    private float whirlwindCooldownTime;
    private bool canCastWirldwind = true;
    private GameObject spawnedWhirlwindObject;

    private float staggerCounter;
    private float staggerFalloff;
    private int maxStaggerCount;

    private float maxYDistToFollow;

    public string displayName = "King Naga";
    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";

    void Awake()
    {
        nagaManager = transform.parent.GetComponent<Naga_Manager>();
        sound = GetComponent<AI_Soundmanager>();
        Initialize();
    }
    void Start()
    {
        movVelocity = 3;
        aggroRange = 10;

        simpleAttackRange = 1.5f;
        cantAttackTimeAfterSimpleAttack = 0.25f;

        whirlwindWindupTime = .5f;
        whirlwindCastTime = 2f;
        immobilizedTimeAfterWhirlwind = 1f;
        whirlwindCooldownTime = 5f;

        staggerVelocity = 0.75f;
        staggerFalloff = 2;
        maxStaggerCount = 2;

        maxYDistToFollow = 3;

        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;
        SetIdle();

        stateAfterAttackCall = () => SetAggro();
        stateAfterStaggeredCall = () => SetAggro();
        if (!startFacingLeft)
        {
            ChangeDirection(true);
        }
    }

    void Update()
    {
        if (staggerCounter > 0)
            staggerCounter -= Time.deltaTime * staggerFalloff;

        switch (state)
        {
            case State.Aggro:
                {
                    bool inLineWithPlayer = RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask) ||
                                            RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask);
                    float dist = Vector2.Distance(transform.position, target.position);
                    float yDistToTarget = target.position.y - coll.bounds.center.y;

                    if (Mathf.Abs(yDistToTarget) > maxYDistToFollow)
                        SetRunning();

                    if (dist <= simpleAttackRange && canAttackSimple)
                    {
                        AttackSimple();
                    }
                    else if (canCastWirldwind)
                    {
                        AttackWhirlwind();
                    }
                    break;
                }
            case State.Idle:
                break;
            case State.Running:
                {
                    float yDistToTarget = target.position.y - coll.bounds.center.y;
                    if (Mathf.Abs(yDistToTarget) < maxYDistToFollow)
                        SetAggro();
                        break;
                }

        }
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Aggro:
                {
                    bool b = target.position.x < coll.bounds.center.x;
                    if (isDirRight == b)
                        ChangeDirection(!b);

                    int direction = isDirRight ? 1 : -1;
                    rb.velocity = new Vector2(direction * movVelocity, rb.velocity.y);

                    break;
                }
            case State.Idle:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Running:
                {
                    bool b = target.position.x < coll.bounds.center.x;
                    if (isDirRight == b)
                        ChangeDirection(!b);
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    break;
                }
            case State.Attacking:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Dead:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Immobilized:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }

    void AttackWhirlwind()
    {
        StartCoroutine(CastWhirlwind(whirlwindWindupTime, whirlwindCastTime, whirlwindCooldownTime));
    }

    IEnumerator CastWhirlwind(float windUpTime, float castTime, float cooldownTime)
    {
        animator.SetBool("IsWindingUpWhirlwind", true);
        SetImmobilized();
        yield return new WaitForSecondsRealtime(windUpTime);

        spawnedWhirlwindObject = Instantiate(whirlwindEffectPrefab, transform.parent);
        spawnedWhirlwindObject.GetComponent<NagaKingWhirlwindEffect>().Set(coll.bounds.center, castTime);
        damageContainer.SetDamageCall(() => whirlwindAttackDamage);
        animator.SetBool("IsChannellingWhirlwind", true);
        animator.SetBool("IsWindingUpWhirlwind", false);
        SetImmobilized();
        yield return new WaitForSecondsRealtime(castTime);

        animator.SetBool("IsChannellingWhirlwind", false);
        damageContainer.SetDamageCall(() => touchDamage);

        canCastWirldwind = false;
        yield return new WaitForSecondsRealtime(cooldownTime);
        canCastWirldwind = true;
    }

    void EndWhirlwindAttack()
    {
        damageContainer.SetDamageCall(() => touchDamage);
        StartCoroutine(SetImmobilizedFor(immobilizedTimeAfterWhirlwind, () => SetRunning()));
    }
    private IEnumerator SetImmobilizedFor(float time, System.Action stateAfter)
    {
        SetImmobilized();
        yield return new WaitForSecondsRealtime(time);
        stateAfter.Invoke();
    }

    void AttackSimple()
    {
        damageContainer.SetDamageCall(() => simpleAttackDamage);
        SetAttack1();
    }
    void EndSimpleAttack()
    {
        SetAggro();
        StartCoroutine(CantSimpleAttack(cantAttackTimeAfterSimpleAttack));
    }
    private IEnumerator CantSimpleAttack(float time)
    {
        canAttackSimple = false;
        yield return new WaitForSecondsRealtime(time);
        canAttackSimple = true;
    }

    public void TeleportToMiddle()
    {
        Vector2 leftHitPoint = Physics2D.Raycast(coll.bounds.center, Vector2.left, 1000, terrainMask).point;
        Vector2 rightHitPoint = Physics2D.Raycast(coll.bounds.center, Vector2.right, 1000, terrainMask).point;
        Vector2 midPoint = (leftHitPoint + rightHitPoint) / 2;
        Vector2 position = Physics2D.Raycast(midPoint, Vector2.down, 1000, terrainMask).point;
        Vector2 offset = (Vector3.up * coll.bounds.extents.y) + transform.position - coll.bounds.center;
        transform.position = position + offset;
    }

    public void Aggro()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        nagaManager.ShowHealthbar(false);
        nagaManager.UpdateHealthbar(false, health, maxHealth);
        SetAggro();
    }
    public void Enrage()
    {
        //TeleportToMiddle();
        state = State.Dead;
        rb.velocity = Vector2.zero;
        animator.SetBool("IsChannellingWhirlwind", false);
        animator.SetBool("IsWindingUpWhirlwind", false);
        animator.SetTrigger("Transform");
        StopAllCoroutines();
        StartCoroutine(StartIncreasingScale(0.5f, 2f, 2f));
    }
    private void EnterRageState()
    {
        Instantiate(enragedPrefab, transform.position, Quaternion.identity, transform.parent);
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private IEnumerator StartIncreasingScale(float delay, float time, float scale)
    {
        yield return new WaitForSecondsRealtime(delay);
        Vector2 targetScale = new Vector2(isDirRight ? -scale : scale, scale);
        while (transform.localScale.x < 2)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, targetScale, Time.deltaTime / time);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    protected void GetHit(bool isRight, float damage)
    {
        if (state == State.Dead)
            return;
        health -= damage;
        if (health <= 0)
        {
            StopAllCoroutines();
            nagaManager.EnrageFemaleNaga();
            SetDead(isRight, this.GetType(), 0);
            nagaManager.Died(false);
            animator.SetBool("IsChannellingWhirlwind", false);
            animator.SetBool("IsWindingUpWhirlwind", false);
            this.enabled = false;
            nagaManager.HideHealthbar(false);
            if(spawnedWhirlwindObject != null)
                spawnedWhirlwindObject.GetComponent<NagaKingWhirlwindEffect>().Stop();
            target.GetComponent<Player>().AddEnemyKilldedToCount(this.GetType());
        }
        else
        {
            bool stagger = false;
            if (staggerCounter + 1 <= maxStaggerCount)
                stagger = true;

            if (stagger)
            {
                staggerCounter += 1;
                SetStaggered(isRight);
            }
            nagaManager.UpdateHealthbar(false, health, maxHealth);
        }
        sound.PlayOneShot(getHitSound);
        cameraController.Shake(damage);
        ParticleEffectManager.PlayEffect(ParticleEffect.Type.blood, coll.bounds.center, isRight ? Vector3.left : Vector3.right);
    }

    protected void updateRune()
    {
        if(target.GetComponent<Player>().GetEnemyKillCount(this.GetType()) > 0)
        {
            GetComponent<LootTable>().rune = null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerWeaponTag) && state != State.Dead)
        {
            DamageContainer damageContainer = other.GetComponentInParent<DamageContainer>();
            GetHit(transform.position.x < other.transform.position.x,
                damageContainer.GetDamage());
        }
    }
    void PlaySimpleAttackEffect()
    {
        cameraController.Shake(3);
        sound.PlayOneShot(simpleAttackSound);
    }
    void PlayWhirlwindAttackEffect()
    {
        cameraController.Shake(3);
        sound.PlayOneShot(whirlwindAttackSound);
    }
}
