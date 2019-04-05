using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Executioner : AI_Base
{
    public GameObject shockwavePrefab;
    public GameObject forceFieldPrefab;
    public float attackDamage;
    public float shockwaveDamage;
    public float stronkAttackDamage;
    public string displayName = "Executioner";
    public AudioClip music;
    public AudioClip attackHitGroundSound;
    public AudioClip stronkAttackHitGroundSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip getHitSound;
    public AudioClip footStepSound;
    public AudioClip shoutLoopSound;
    public AudioClip slamIntoWallSound;

    private AI_Soundmanager sound;
    private SoundManager soundManager;
    private AreaNotificationText notificationText;
    private EnemyBossHealthbar bossHealthbar;
    private LayerMask terrainMask;
    private LayerMask playerMask;
    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";

    private float stronkAttackRange;
    private float stronkAttackImmobilizedTime;
    private float stronkAttackWindUpTime;
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

    private float maxShoutRange;
    private float shoutTime;
    private float shoutCounter;
    private int maxShoutCount;

    private float staggerFalloff;
    private float staggerCounter;
    private int maxStaggerCount;

    private bool hasBeenAggroed;

    void Awake()
    {
        sound = GetComponent<AI_Soundmanager>();
        soundManager = GameObject.FindObjectOfType<SoundManager>();
        notificationText = GameObject.FindObjectOfType<AreaNotificationText>();
        bossHealthbar = GameObject.FindObjectOfType<EnemyBossHealthbar>();
        Initialize();
    }

    void Start()
    {
        // stats ----------------------------------------
        movVelocity = 5;
        aggroRange = 10;
        knockBackVelocity = 1;
        staggerVelocity = 0.5f;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");
        landImmobalizedTime = 2f;
        minJumpInterval = .5f;

        stronkAttackRange = 3;
        stronkAttackImmobilizedTime = 1f;
        stronkAttackWindUpTime = 0.75f;
        attackRange = 2.5f;
        attackImmobilizedTime = 0.5f;

        jumpWindUpTime = 0.75f;

        maxChargeRange = 6;
        minChargeRange = stronkAttackRange * 1.2f;
        chargeWindUpTime = .75f;
        maxChargeTime = 3f;
        wallStunTime = 1.5f;
        
        maxJumpRange = 10f;
        minJumpRange = 5f;

        shoutTime = 1f;
        maxShoutRange = 1.5f;
        maxShoutCount = 2;

        staggerFalloff = 1;
        maxStaggerCount = 3;

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

        if (shoutCounter > 0)
            shoutCounter -= Time.deltaTime;

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
                        if (dist < maxShoutRange && shoutCounter + 1 <= maxShoutCount)
                            ShoutAttack();
                        else if (dist < maxChargeRange && dist > minChargeRange)
                            ChargeStart();
                        else if (dist < maxJumpRange && dist > minJumpRange)
                            AttackJump();
                        else if (dist < attackRange)
                            AttackNormal();
                        else if (dist < stronkAttackRange)
                            AttackStronk();
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
                    if (!hasBeenAggroed)
                    {
                        notificationText.ShowNotification(displayName);
                        soundManager.PlayBossMusic(music);
                        ChangeDirection(coll.bounds.center.x < target.position.x);
                        ShoutAttack();
                        bossHealthbar.Show();
                        hasBeenAggroed = true;
                    }
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
                    if (isGrounded && minJumpTimer <= 0)
                        Land();
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
    void ShoutAttack()
    {
        StartCoroutine(ShoutFor(shoutTime));
    }
    IEnumerator ShoutFor(float time)
    {
        shoutCounter += 2;
        animator.SetBool("Shout", true);
        Instantiate(forceFieldPrefab, transform.parent).GetComponent<ForceField>().Set(coll.bounds.center, 3, time);
        SetImmobilized();
        sound.PlayFor(shoutLoopSound, time, 0.1f);
        yield return new WaitForSecondsRealtime(time);
        animator.SetBool("Shout", false);
        SetAggro();
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
    }
    void Land()
    {
        Vector2 shockwaveOrigin = coll.bounds.center;
        shockwaveOrigin.y = coll.bounds.min.y;
        Instantiate(shockwavePrefab, transform.parent).GetComponent<Shockwave>().Set(shockwaveOrigin, 3f, Vector2.right, shockwaveDamage);
        Instantiate(shockwavePrefab, transform.parent).GetComponent<Shockwave>().Set(shockwaveOrigin, 3f, Vector2.left, shockwaveDamage);

        cameraController.Shake(10);
        StartCoroutine(SetImmobilizeFor(landImmobalizedTime));
        sound.PlayOneShot(landSound);
    }
    void AttackStronk()
    {
        StartCoroutine(StronkAttackWindUp(stronkAttackWindUpTime));
    }
    IEnumerator StronkAttackWindUp(float time)
    {
        animator.SetBool("StronkAttackWindUp", true);
        ChangeDirection(coll.bounds.center.x < target.position.x);
        SetImmobilized();
        yield return new WaitForSecondsRealtime(time);
        damageContainer.SetDamageCall(() => stronkAttackDamage);
        animator.SetBool("StronkAttackWindUp", false);
        SetAttack2();
    }
    void AttackNormal()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        damageContainer.SetDamageCall(() => attackDamage);
        SetAttack1();
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
    }
    void EndCharge(bool attack, bool stun)
    {
        if (attack)
            AttackNormal();
        else if (stun)
        {
            sound.PlayOneShot(slamIntoWallSound);
            cameraController.Shake(10);
            StartCoroutine(Stunned(wallStunTime, () => SetAggro()));
        }
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
                    bool b = target.position.x < coll.bounds.center.x;
                    if (isDirRight != b)
                        ChangeDirection(!b);

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
        if (state == State.Dead)
            return;
        health -= damage;
        print(name + " Health: " + health);
        if (health <= 0)
        {
            SetDead(isRight);
            StopAllCoroutines();
            animator.SetBool("Shout", false);
            animator.SetBool("Charging", false);
            animator.SetBool("Stunned", false);
            soundManager.StopPlayingBossMusic();
            bossHealthbar.Hide();
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
        bossHealthbar.UpdateHealthbar(health, maxHealth);
        sound.PlayOneShot(getHitSound);
        cameraController.Shake(damage);
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

    // sound events
    void PlayFootstep()
    {
        sound.PlayOneShot(footStepSound);
        cameraController.Shake(3);
    }

    void PlayAttackEffect()
    {
        sound.PlayOneShot(attackHitGroundSound);
        cameraController.Shake(3);
    }
    void PlayStronkAttackEffect()
    {
        sound.PlayOneShot(stronkAttackHitGroundSound);
        cameraController.Shake(20);
    }
}
