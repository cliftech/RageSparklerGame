using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_MaleNagaEnraged : AI_Base
{
    private Naga_Manager nagaManager;
    private AI_Soundmanager sound;
    private LayerMask terrainMask;
    private LayerMask playerMask;

    public AudioClip simpleAttackSound;
    public AudioClip whirlwindAttackSound;
    public AudioClip getHitSound;
    public AudioClip shockwaveExplosionSound;
    public GameObject whirlwindEffectPrefab;
    public GameObject explosionBeforeShockwavePrefab;
    public GameObject shockwavePrefab;

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

    public float shockwaveDamage;

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
        if (sound == null)
            Awake();
        movVelocity = 4;
        aggroRange = 10;

        simpleAttackRange = 1.75f;
        cantAttackTimeAfterSimpleAttack = 0.4f;

        whirlwindWindupTime = 1f;
        whirlwindCastTime = 3f;
        immobilizedTimeAfterWhirlwind = 0.5f;
        whirlwindCooldownTime = 5f;

        staggerVelocity = 0.75f;
        staggerFalloff = 2;
        maxStaggerCount = 2;

        maxYDistToFollow = 3;

        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;

        stateAfterAttackCall = () => SetAggro();
        stateAfterStaggeredCall = () => SetAggro();
        ChangeDirection(coll.bounds.center.x < target.position.x);
        SetAggro();
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

        Instantiate(whirlwindEffectPrefab, transform.parent).GetComponent<NagaKingWhirlwindEffect>().Set(coll.bounds.center, castTime);
        damageContainer.SetDamageCall(() => whirlwindAttackDamage);
        animator.SetBool("IsChannellingWhirlwind", true);
        animator.SetBool("IsWindingUpWhirlwind", false);
        SetImmobilized();
        PlayWhirlwindAttackEffect();
        yield return new WaitForSecondsRealtime(castTime);

        animator.SetBool("IsChannellingWhirlwind", false);
        damageContainer.SetDamageCall(() => touchDamage);

        Instantiate(explosionBeforeShockwavePrefab, coll.bounds.center, Quaternion.identity, transform.parent).GetComponent<ParticleSystem>().Play();
        Vector2 shockwaveOrigin = coll.bounds.center;
        shockwaveOrigin.y = coll.bounds.min.y;
        Instantiate(shockwavePrefab, transform.parent).GetComponent<Shockwave>().Set(shockwaveOrigin, 3f, Vector2.right, shockwaveDamage);
        Instantiate(shockwavePrefab, transform.parent).GetComponent<Shockwave>().Set(shockwaveOrigin, 3f, Vector2.left, shockwaveDamage);

        sound.PlayOneShot(shockwaveExplosionSound);

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

    protected void GetHit(bool isRight, float damage)
    {
        if (state == State.Dead)
            return;
        health -= damage;
        if (health <= 0)
        {
            StopAllCoroutines();
            SetDead(isRight);
            nagaManager.Died(false);
            animator.SetBool("IsChannellingWhirlwind", false);
            animator.SetBool("IsWindingUpWhirlwind", false);
            nagaManager.StopPlayingBossMusic();
            nagaManager.HideHealthbar(false);
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
