using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AI_FemaleNagaEnraged : AI_Base
{
    private Naga_Manager nagaManager;
    private AI_Soundmanager sound;
    private BossArenaPlatforms platforms;
    private LayerMask terrainMask;
    private LayerMask playerMask;

    public GameObject explosionPrefab;

    public AudioClip rangedAttackHitSound;
    public AudioClip pierceAttackHitSound;
    public AudioClip getHitSound;

    private Transform[] tpPositions;
    private Transform originalParent;

    public float staffAttackDamage;
    public float rangedAttackDamage;
    public float pierceAttackDamage;
    private float rangedAttackRange;
    private float pierceAttackRange;
    private float timeBetweenRanged;
    private float immobilizeTime;
    private bool canRangedAttack = true;
    private bool canTakeDamage = true;
    public string displayName = "Queen Naga";
    private string playerTag = "Player";
    private string playerWeaponTag = "PlayerWeapon";

    private int currTpIndex;
    void Awake()
    {
        nagaManager = FindObjectOfType<Naga_Manager>();
        originalParent = transform.parent;
        platforms = Resources.FindObjectsOfTypeAll<BossArenaPlatforms>()[0];
        sound = GetComponent<AI_Soundmanager>();

        Initialize();
    }
    void Start()
    {
        aggroRange = 5;
        rangedAttackRange = 50;
        pierceAttackRange = 3;
        timeBetweenRanged = 0.5f;
        immobilizeTime = 7.5f;
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        playerMask = 1 << LayerMask.NameToLayer("Player");

        damageContainer.SetDamageCall(() => touchDamage);
        health = maxHealth;

        stateAfterAttackCall = () => SetAggro();
        stateAfterStaggeredCall = () => SetAggro();

        tpPositions = platforms.platformTpPossitions;
        StartCoroutine(SetImmobilizedAtStart(immobilizeTime, () => SetAggro()));
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, target.position);
        switch (state)
        {
            case State.Aggro:
                bool inLineWithPlayer = RaycastToPlayer(isDirRight, aggroRange, playerTag, playerMask, terrainMask) ||
                                        RaycastToPlayer(!isDirRight, aggroRange, playerTag, playerMask, terrainMask);
                if (dist <= pierceAttackRange)
                {
                    AttackPierce();
                }
                else if (dist <= rangedAttackRange && canRangedAttack)
                {
                    AttackRanged();
                }
                break;
        }
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
    }
    void AttackRanged()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        damageContainer.SetDamageCall(() => staffAttackDamage);
        SetAttack1();
    }
    void AttackPierce()
    {
        ChangeDirection(coll.bounds.center.x < target.position.x);
        damageContainer.SetDamageCall(() => pierceAttackDamage);
        SetAttack2();
    }
    void EndAttackRanged()
    {
        StartCoroutine(DisableRanged(timeBetweenRanged));
        SetAggro();
    }
    private IEnumerator DisableRanged(float time)
    {
        canRangedAttack = false;
        yield return new WaitForSecondsRealtime(time);
        canRangedAttack = true;
    }
    void EndAttackPierce()
    {
        damageContainer.SetDamageCall(() => touchDamage);
        SetAggro();
    }
    void Teleport()
    {
        animator.SetTrigger("Teleport");
        SetImmobilized();
    }
    void TeleportEvent()
    {
        int index = 0;
        do { index = Random.Range(0, tpPositions.Length); } while (index == currTpIndex);
        Vector2 position = Physics2D.Raycast(tpPositions[index].position, Vector2.down, 1000, terrainMask).point;
        Vector2 offset = (Vector3.up * coll.bounds.extents.y) + transform.position - coll.bounds.center;
        transform.position = position + offset;
        currTpIndex = index;
    }
    void EndTeleport()
    {
        SetAggro();
    }
    private IEnumerator SetImmobilizedAtStart(float time, System.Action stateAfter)
    {
        SetImmobilized();
        canTakeDamage = false;
        yield return new WaitForSecondsRealtime(time);
        stateAfter.Invoke();
        canTakeDamage = true;
    }
    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Aggro:
                if (target.position.x < coll.bounds.center.x)
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
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Attacking:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
            case State.Idle:
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
    protected void GetHit(bool isRight, float damage)
    {
        if (state == State.Dead || !canTakeDamage)
            return;
        health -= damage;
        if (health <= 0)
        {
            StopAllCoroutines();
            SetDead(isRight,this.GetType());
            nagaManager.Died(true);
            nagaManager.StopPlayingBossMusic();
            nagaManager.HideHealthbar(true);
            target.GetComponent<Player>().AddEnemyKilledToCount(this.GetType());
        }
        Teleport();
        nagaManager.UpdateHealthbar(true, health, maxHealth);
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
    void PlayRangedAttackEffect()
    {
        cameraController.Shake(3);
        sound.PlayOneShot(rangedAttackHitSound);
    }
    void PlayPierceAttackEffect()
    {
        cameraController.Shake(3);
        sound.PlayOneShot(pierceAttackHitSound);
    }
    void SpawnExplosion()
    {
        Instantiate(explosionPrefab, target.position, Quaternion.identity, originalParent).GetComponent<NagaQueenExplosionEffect>().Set(target.position, rangedAttackDamage);
    }
}
