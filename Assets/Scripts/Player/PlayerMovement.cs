using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    private PlayerSoundController playerSoundController;

    private float movVelocity = 5;
    private float acceleration = 1;
    private float jumpVel = 10;
    private float inAirAttackUpwardVel = 2.25f;
    private float downwardAttackDownwardVel = 2f;
    private float groundedAttackHorizontalVel = 2f;
    private float slamPushVel = 3;
    private float inAirJumpVelMult = 0.8f;
    private float dashSpeed = 10f;
    private float stuckToWall_g_mult = 0.5f;
    private float endJumpMult = 0.5f;
    private float knockBackVel = 6;
    private float spriteFlashFrequency = .05f;
    public LayerMask groundMask;
    public LayerMask wallMask;
    public LayerMask wallStickMask;
    public LayerMask unDashableMask;
    public LayerMask slamPushableMask;
    public LayerMask unslideableWhileAttackingMask;
    private string enemyLayerName = "Enemy";
    private int enemyLayer;
    private string enemyWeaponLayerName = "EnemyWeapon";
    private int enemyWeaponLayer;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public CapsuleCollider2D capsColl;
    [HideInInspector] public DamageContainer damageContainer;
    [HideInInspector] public Player player;
    private float horizontalInput;
    private float verticalInput;
    [Header("These will be unlocked/upgraded by progressing")]
    public bool dashUnlocked;
    public bool midAirDashUnlocked;
    public bool downwardAttackUnlocked;
    public bool wallJumpingUnlocked;
    public int maxJumpCount = 2;
    public float dashDistance = 3;
    public float minDelayBetweenDashes = 0.2f;
    public int maxMidairDashesCount = 1;
    public float invincibilityFrameTime = 0.5f;
    [Header("states (for debuging)")]
    [SerializeField] public bool isGrounded;
    [SerializeField] private bool isDashing;
    [SerializeField] private bool isAttacking;
    [SerializeField] public bool isDownwardAttacking;
    [SerializeField] private bool isStuckToWall_L;
    [SerializeField] private bool isStuckToWall_R;
    [SerializeField] private bool isKnockedBack;
    [SerializeField] public bool isInvulnerable;
    [SerializeField] public bool isDisabled;
    private bool hasSlowedDownFromSticking;
    private Vector2 dashPos;
    private float gravityScale;
    private float accel;
    private bool isDirRight;
    private float knockBackTime = 0.5f;
    private float knockBackTimer;
    private float attackCooldownTime = .2f;
    private float attackCooldownTimer;
    private float dashTimer;
    private int jumpCounter;
    private int attackComboCount;
    [HideInInspector] public int currentAttackNum;
    private int midairDashCounter;
    private IEnumerator spriteFlashRoutine;
    private Color normalSpriteColor;
    private Vector2 originalScale;

    private float yRaylength;
    private float xRaylength;

    void Awake ()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capsColl = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageContainer = GetComponent<DamageContainer>();
        player = GetComponent<Player>();
        gravityScale = rb.gravityScale;
    }
    void Start()
    {
        yRaylength = capsColl.bounds.size.y / 2 + 0.05f;
        xRaylength = capsColl.bounds.size.x / 2 + 0.1f;
        accel = acceleration;
        isDirRight = true;
        enemyWeaponLayer = LayerMask.NameToLayer(enemyWeaponLayerName);
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        originalScale = transform.localScale;
    }
	
	void Update ()
    {
        if (player.isDead)
            return;
        bool newGrounded = IsGrounded();
        if (!isGrounded && newGrounded)
            Land();
        isGrounded = newGrounded;

        if (!isDisabled)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }

        if (wallJumpingUnlocked && (!isGrounded && (!isStuckToWall_L && !isStuckToWall_R)))
        {
            if (CanSlideLeft() && horizontalInput < 0)
                StickToWall(-1);
            else if (CanSlideRight() && horizontalInput > 0)
                StickToWall(1);
        }

        if (Input.GetButtonDown("Jump") && !isDownwardAttacking && jumpCounter < maxJumpCount - 1 && !isDashing &&!isDisabled)
            Jump();
        else if (Input.GetButtonUp("Jump"))
            EndJump();
        if (dashUnlocked && (!isDashing && dashTimer == 0 && midairDashCounter < maxMidairDashesCount && !isDownwardAttacking && (knockBackTimer < knockBackTime / 2f)) && !isDisabled)
        {
            if (isGrounded || midAirDashUnlocked)
            {
                if (Input.GetButtonDown("DashLeft") && !isStuckToWall_L)
                    DashLeft();
                if (Input.GetButtonDown("DashRight") && !isStuckToWall_R)
                    DashRight();
            }
        }
        else if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                dashTimer = 0;
        }
        if (((isGrounded && attackComboCount < 3) || (!isGrounded && attackComboCount < 2)) && !isDisabled && attackCooldownTimer <= 0 && !isDashing && !isDownwardAttacking && ! isStuckToWall_L && ! isStuckToWall_R && !isKnockedBack)
        {
            if (Input.GetButtonDown("Attack")) {
                if (isGrounded)
                    Attack();
                else
                {
                    if (verticalInput >= 0)
                        AirAttack();
                    else if (downwardAttackUnlocked)
                        DownwardAttack();
                }
            }
        }
        
        if (isKnockedBack)
        {
            knockBackTimer -= Time.fixedDeltaTime;
            if (knockBackTimer <= 0)
                EndKnockBack();
        }

        // setting animator parameters
        animator.SetFloat("Horizontal Velocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("Vertical Velocity", rb.velocity.y);
        animator.SetBool("Is Grounded", isGrounded);
        animator.SetBool("Is Wall Sliding", isStuckToWall_L || isStuckToWall_R);
        animator.SetFloat("Wall Sliding Speed", 0.5f + rb.gravityScale / gravityScale);
        animator.SetFloat("Slide Stand Speed", horizontalInput == 0 ? 1 : 10);
        animator.SetInteger("Attack Combo Count", attackComboCount);
    }

    void FixedUpdate()
    {
        if (player.isDead)
            return;
        if (isKnockedBack)
        {
        }
        else if (isDashing)
        {
            if (isGrounded)
                transform.position = Vector2.Lerp(transform.position, dashPos, dashSpeed * Time.fixedDeltaTime);
            else
                transform.position = Vector2.MoveTowards(transform.position, dashPos, dashSpeed * Time.fixedDeltaTime);
            if (GetHorizontalDistance(transform.position, dashPos) < 0.1f)
                StopDashing();
            Debug.DrawLine(transform.position, dashPos, Color.white, 0.075f);
        }
        else if (isStuckToWall_L || isStuckToWall_R)
        {
            if (!hasSlowedDownFromSticking)
            {
                if (rb.velocity.y < 0)
                {
                    rb.gravityScale = 0;
                    rb.velocity = Vector2.zero;
                    hasSlowedDownFromSticking = true;
                }
            }
            else
                rb.gravityScale = Mathf.MoveTowards(rb.gravityScale, gravityScale * stuckToWall_g_mult, Time.fixedDeltaTime);

            if ((CanSlideLeft() && horizontalInput > 0) || (CanSlideRight() && horizontalInput < 0) || (!CanSlideLeft() && !CanSlideRight()))
                UnstickFromWall();
            else
                TeleportToWall(isStuckToWall_R);
        }
        else if (isAttacking)
        {
            if (!isGrounded)
            {
                if (horizontalInput == 0 || (horizontalInput > 0 && CanGoRight()) || (horizontalInput < 0 && CanGoLeft()))
                    rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(horizontalInput * movVelocity, rb.velocity.y), acceleration);
            }
            else
            {
                Vector2 origin = capsColl.bounds.center;
                if (!DoesGroundInFrontExists() || RaycastSideways_OR((isDirRight ? 1 : -1), 3, Color.red, unslideableWhileAttackingMask))
                    rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), acceleration);
            }
            attackCooldownTimer -= Time.fixedDeltaTime;
        }
        else if (isDownwardAttacking)
        {
            // push rigid bodies that are beneath the PC
            PushRigidBodies();
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), acceleration);
        }
        else
        {
            if (horizontalInput == 0 || (horizontalInput > 0 && CanGoRight()) || (horizontalInput < 0 && CanGoLeft()))
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(horizontalInput * movVelocity, rb.velocity.y), acceleration);
            else
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), acceleration);

            // flip character if going in a differrent direction
            if (!isDashing)
            {
                if (horizontalInput > 0 && !isDirRight)
                    SetDirFacing(true);
                else if (horizontalInput < 0 && isDirRight)
                    SetDirFacing(false);
            }
        }
        if(acceleration < accel)
            acceleration = Mathf.MoveTowards(acceleration, accel, Time.fixedDeltaTime);

        if (!isStuckToWall_L && !isStuckToWall_R && !isDashing && !isAttacking && !isDownwardAttacking && !isKnockedBack)
        {
            if (IsOnCorner())
                rb.gravityScale = 0;
            else
                rb.gravityScale = gravityScale;
        }
    }

    #region attacking
    void Attack()
    {
        animator.SetTrigger("Attack");
        isAttacking = true;
        attackComboCount++;
        if (isGrounded)
            rb.velocity = new Vector2(rb.velocity.x / 2, rb.velocity.y);
        attackCooldownTimer = attackCooldownTime;
    }
    //called in animator events
    public void SlideForwardDuringAttack()
    {
        currentAttackNum++;

        Vector2 origin = capsColl.bounds.center;
        //if (DoesGroundInFrontExists() && !Physics2D.Raycast(origin, Vector2.right * (isDirRight ? 1 : -1), xRaylength, unDashableMask))
            rb.velocity = new Vector2((isDirRight ? 1 : -1) * groundedAttackHorizontalVel, rb.velocity.y);
    }
    //called in animator events
    public void EndAttack()
    {
        isAttacking = false;
        isDownwardAttacking = false;
        if (isGrounded)
        {
            attackComboCount = 0;
            currentAttackNum = 0;
            attackCooldownTimer = 0;
        }
    }
    void ForceEndAttack()
    {
        isAttacking = false;
        isDownwardAttacking = false;
        attackComboCount = 0;
        attackCooldownTimer = 0;
        currentAttackNum = 0;
    }
    void AirAttack()
    {
        animator.SetTrigger("Attack");
        isAttacking = true;
        rb.velocity = new Vector2(rb.velocity.x, inAirAttackUpwardVel);
        attackComboCount++;
    }
    public void AirAttackCommence()
    {
        currentAttackNum++;
    }
    void DownwardAttack()
    {
        animator.SetTrigger("Downward Attack");
        isDownwardAttacking = true;
        rb.velocity = new Vector2(rb.velocity.x, 
            rb.velocity.y < -downwardAttackDownwardVel ? rb.velocity.y : -downwardAttackDownwardVel);
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyWeaponLayer, true);

        player.soundController.PlayDownwardAttackStart();
    }
    /// <summary>
    /// Pushes all rigid bodies away from PC while mid air
    /// </summary>
    void PushRigidBodies()
    {
        Vector2 origin = capsColl.bounds.center;
        origin.y = capsColl.bounds.min.y - 0.25f;
        float pushDistance = capsColl.size.x / 1.9f;
        Debug.DrawLine(origin + Vector2.right * pushDistance, origin - Vector2.right * pushDistance, Color.red, 0.1f);
        RaycastHit2D[] hits = Physics2D.LinecastAll(origin - Vector2.right * pushDistance, origin + Vector2.right * pushDistance, slamPushableMask);
        foreach (RaycastHit2D hit in hits)
        {
            Rigidbody2D hitRb = hit.collider.GetComponent<Rigidbody2D>();
            Vector2 pushDir = hitRb.transform.position.x > transform.position.x ? Vector2.right : Vector2.left;
            hitRb.velocity = pushDir * slamPushVel;
        }
    }
    #endregion
    #region dashing
    void DashLeft()
    {
        SetDirFacing(false);
        Dash(-1);
    }
    void DashRight()
    {
        SetDirFacing(true);
        Dash(1);
    }
    void Dash(int direction)
    {
        Vector2 origin = capsColl.bounds.center;
        int numOfRays = 5;
        float yIncrament = capsColl.size.y / numOfRays;
        origin.y -= capsColl.size.y / 2f;

        Vector2 defaultPos = Vector3.right * direction * dashDistance + transform.position;
        dashPos = defaultPos;
        for (int i = 0; i < numOfRays + 1; i++)
        {
            Debug.DrawRay(origin, Vector2.right * direction * dashDistance, Color.white, 0.15f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, dashDistance, unDashableMask);
            Vector2 hitPos = defaultPos;
            if (hit)
            {
                hitPos = new Vector2(hit.point.x - (direction * capsColl.size.x / 2f), transform.position.y);
                dashPos = Vector2.Distance(transform.position, hitPos) < Vector2.Distance(transform.position, dashPos) ?
                    hitPos : dashPos;
            }
            origin.y += yIncrament;
        }

        if (!isGrounded)
            midairDashCounter++;
        isDashing = true;
        isStuckToWall_L = false;
        isStuckToWall_R = false;
        ForceEndAttack();
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        animator.SetBool("Dashing", isGrounded);
        animator.SetBool("Rolling", !isGrounded);

        player.soundController.PlayDashSound();
    }
    public void StopDashing()
    {
        if(isDashing)
            dashTimer = minDelayBetweenDashes;
        isDashing = false;
        rb.gravityScale = gravityScale;

        animator.SetBool("Dashing", false);
        animator.SetBool("Rolling", false);
    }
    #endregion
    #region jumping
    void Jump()
    {
        Vector3 jumpDir;
        if (isStuckToWall_L || isStuckToWall_R)
        {
            jumpDir = CanSlideLeft() ? new Vector2(1, 1) : new Vector2(-1, 1);
            acceleration = 0;
        }
        else
            jumpDir = Vector2.up;
        jumpDir.Normalize();

        rb.velocity = jumpDir * (isGrounded || isStuckToWall_L || isStuckToWall_R ? jumpVel : jumpVel * inAirJumpVelMult);
        if(!isGrounded && !isStuckToWall_L && !isStuckToWall_R)
            jumpCounter++;
        UnstickFromWall();

        animator.SetTrigger("Jump");
        ForceEndAttack();

        player.soundController.PlayJumpSound();
    }
    void EndJump()
    {
        if(rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * endJumpMult);
    }
    void Land()
    {
        UnstickFromWall();
        StopDashing();
        jumpCounter = 0;
        midairDashCounter = 0;
        attackComboCount = 0;
        ForceEndAttack();
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyWeaponLayer, false);

        player.soundController.PlayLandSound();
    }
    #endregion
    #region sticking to walls
    void StickToWall(int direction)
    {
        isStuckToWall_L = direction == -1 ? true : false;
        isStuckToWall_R = !isStuckToWall_L;

        TeleportToWall(isStuckToWall_R);

        jumpCounter = 0;
        midairDashCounter = 0;
        SetDirFacing(direction == 1);
    }
    void TeleportToWall(bool toRightWall)
    {
        int direction = toRightWall ? 1 : -1;
        Vector2 origin = transform.position;
        Debug.DrawRay(origin, Vector2.right * direction * xRaylength, Color.yellow, 0.075f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, wallStickMask);
        if (hit)
            transform.position = hit.point + (Vector2.right * capsColl.size.x / 2f * -direction);
    }
    void UnstickFromWall()
    {
        if(isStuckToWall_L || isStuckToWall_R)
            SetDirFacing(isStuckToWall_L);
        isStuckToWall_L = false;
        isStuckToWall_R = false;
        hasSlowedDownFromSticking = false;
        rb.gravityScale = gravityScale;
    }
    #endregion

    public void KnockBack(bool knockbackToLeftSide, float forceMult)
    {
        Vector2 forceDir = (knockbackToLeftSide ? new Vector2(-1, .5f) : new Vector2(1, .5f)).normalized;
        rb.velocity = forceDir * knockBackVel * Mathf.Clamp(forceMult / 100, 1, 1.5f);
        knockBackTimer = knockBackTime;
        isKnockedBack = true;
        isDownwardAttacking = false;
        isStuckToWall_L = false;
        isStuckToWall_R = false;
        ForceEndAttack();
        StopDashing();
        SetDirFacing(knockbackToLeftSide);

        if (spriteFlashRoutine != null)
        {
            StopCoroutine(spriteFlashRoutine);
            spriteRenderer.color = normalSpriteColor;
            // add enemy layer from undashable
            unDashableMask = unDashableMask | (1 << enemyLayer);
        }
        spriteFlashRoutine = StartInvincibillityFrame(invincibilityFrameTime, spriteFlashFrequency);
        StartCoroutine(spriteFlashRoutine);
        animator.SetTrigger("GetHit");
    }

    public void KnockbackUp(float forceMult)
    {
        Vector2 forceDir = Vector2.up;
        rb.velocity = forceDir * knockBackVel * Mathf.Clamp(forceMult / 100, 1, 1.5f);
        knockBackTimer = knockBackTime;
        isKnockedBack = true;
        isDownwardAttacking = false;
        isStuckToWall_L = false;
        isStuckToWall_R = false;
        ForceEndAttack();
        StopDashing();

        if (spriteFlashRoutine != null)
        {
            StopCoroutine(spriteFlashRoutine);
            spriteRenderer.color = normalSpriteColor;
            // add enemy layer from undashable
            unDashableMask = unDashableMask | (1 << enemyLayer);
        }
        spriteFlashRoutine = StartInvincibillityFrame(invincibilityFrameTime, spriteFlashFrequency);
        StartCoroutine(spriteFlashRoutine);
        animator.SetTrigger("GetHit");
    }

    public void EndKnockBack()
    {
        isKnockedBack = false;
        knockBackTimer = 0;
    }

    public void SetEnabled(bool enabled)
    {
        isDisabled = !enabled;
        horizontalInput = 0;
        verticalInput = 0;
    }

    void SetDirFacing(bool isRight)
    {
        transform.localScale = new Vector3(isRight ? originalScale.x : -originalScale.x, originalScale.y);
        isDirRight = isRight;
    }

    IEnumerator StartInvincibillityFrame(float duration, float flashFrequency)
    {
        // remove enemy layer from undashable
        unDashableMask = unDashableMask & ~(1 << enemyLayer);
        isInvulnerable = true;

        bool currentColor = false;
        normalSpriteColor = spriteRenderer.color;
        Color alternateColor = new Color(1 - normalSpriteColor.r, 1 - normalSpriteColor.g, 1 - normalSpriteColor.b, normalSpriteColor.a / 2);
        while (duration > 0)
        {
            spriteRenderer.color = currentColor ? alternateColor : normalSpriteColor;
            currentColor = !currentColor;
            duration -= flashFrequency;
            yield return new WaitForSecondsRealtime(flashFrequency);
        }
        spriteRenderer.color = normalSpriteColor;

        // add enemy layer from undashable
        unDashableMask = unDashableMask | (1 << enemyLayer);
        isInvulnerable = false;
        capsColl.enabled = false;
        capsColl.enabled = true;
    }

    bool IsGrounded()
    {
        Vector2 origin = capsColl.bounds.center;
        float xOffset = capsColl.bounds.size.x / 3.05f;
        origin.x -= xOffset * 3/2;
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawRay(origin, -Vector2.up * yRaylength, Color.magenta, 0.075f);
            if (Physics2D.Raycast(origin, -Vector2.up, yRaylength, groundMask))
                return true;
            origin.x += xOffset;
        }
        return false;
    }
    bool IsOnCorner()
    {
        Vector2 origin = capsColl.bounds.center;
        float xOffset = capsColl.bounds.extents.x / 1.05f;
        bool centerRay = Physics2D.Raycast(origin, -Vector2.up, yRaylength, groundMask);
        Debug.DrawRay(origin, -Vector2.up * yRaylength, Color.blue, 0.075f);
        origin.x -= xOffset;
        bool leftRay = Physics2D.Raycast(origin, -Vector2.up, yRaylength, groundMask);
        Debug.DrawRay(origin, -Vector2.up * yRaylength, Color.blue, 0.075f);
        origin.x += xOffset * 2;
        bool rightRay = Physics2D.Raycast(origin, -Vector2.up, yRaylength, groundMask);
        Debug.DrawRay(origin, -Vector2.up * yRaylength, Color.blue, 0.075f);

        if (!centerRay && (leftRay || rightRay))
            return true;
        return false;
    }
    bool DoesGroundInFrontExists()
    {
        Vector2 origin = capsColl.bounds.center;
        origin.y = capsColl.bounds.min.y;
        origin.x += (isDirRight ? 1 : -1) * capsColl.bounds.extents.x;

        Debug.DrawRay(origin, -Vector2.up * yRaylength, Color.cyan, 0.075f);
        return (Physics2D.Raycast(origin, -Vector2.up, yRaylength, groundMask));
    }

    bool CanSlideLeft()
    {
        return RaycastSideways_AND(-1, 2, Color.magenta, wallStickMask);
    }
    bool CanSlideRight()
    {
        return RaycastSideways_AND(1, 2, Color.magenta, wallStickMask);
    }

    bool CanGoRight()
    {
        return !RaycastSideways_OR(1, 5, Color.red, wallMask);
    }

    bool CanGoLeft()
    {
        return !RaycastSideways_OR(-1, 5, Color.red, wallMask);
    }


    bool RaycastSideways_OR(int direction, int perpRayCount, Color debugColor, LayerMask mask)
    {
        Vector2 origin = capsColl.bounds.center;
        if (perpRayCount == 1)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRaylength, debugColor, 0.075f);
            return Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, mask);
        }
        float yOffset = (capsColl.size.y - 0.05f) / perpRayCount;
        origin.y -= yOffset * perpRayCount / 2;
        for (int i = 0; i < perpRayCount + 1; i++)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRaylength, debugColor, 0.075f);
            if (Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, mask))
                return true;
            origin.y += yOffset;
        }
        return false;
    }

    bool RaycastSideways_AND(int direction, int perpRayCount, Color debugColor, LayerMask mask)
    {
        Vector2 origin = capsColl.bounds.center;
        if (perpRayCount == 1)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRaylength, debugColor, 0.075f);
            return Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, mask);
        }

        float yOffset = capsColl.size.y / perpRayCount;
        origin.y -= yOffset * perpRayCount / 4;
        for (int i = 0; i < perpRayCount; i++)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRaylength, debugColor, 0.075f);
            if (!Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, mask))
                return false;
            origin.y += yOffset;
        }
        return true;
    }

    float GetHorizontalDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x);
    }
}
