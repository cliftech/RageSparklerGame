using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float movSpeed = 5;
    public float acceleration = 5;
    public float jumpVel = 5;
    public float inAirAttackUpwardVel = 1;
    public float downwardAttackUpwardVel = 1.5f;
    public float inAirJumpVelMult = 0.75f;
    public float dashDistance = 3;
    public float dashSpeed = 20;
    public float stuckToWall_g_mult = 0.75f;
    public float endJumpMult = 0.75f;
    public float minDelayBetweenDashes = 0.5f;
    public int maxJumpCount = 3;
    public int maxMidairDashesCount = 1;

    public LayerMask groundMask;
    public LayerMask wallMask;
    public LayerMask wallStickMask;
    public LayerMask unDashableMask;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private float horizontalInput;
    private float verticalInput;
    [Header("states (for debuging)")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isDashing;
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isStuckToWall_L;
    [SerializeField] private bool isStuckToWall_R;
    private bool hasSlowedDownFromSticking;
    private Vector2 dashPos;
    private float gravityScale;
    private float accel;
    private bool isDirRight;

    private float yRaylength;
    private float xRaylength;

    private float dashTimer;
    private int jumpCounter;
    private int attackComboCount;
    private int midairDashCounter;

    void Awake ()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        yRaylength = coll.size.y / 2 + 0.05f;
        xRaylength = coll.size.x / 2 + 0.1f;
        gravityScale = rb.gravityScale;
        accel = acceleration;
        isDirRight = true;
    }
	
	void Update ()
    {
        bool newGrounded = IsGrounded();
        if (!isGrounded && newGrounded)
            Land();
        isGrounded = newGrounded;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        // flip character if going in a differrent direction
        if (!isDashing)
        {
            if (horizontalInput > 0 && !isDirRight)
                SetDir(true);
            else if (horizontalInput < 0 && isDirRight)
                SetDir(false);
        }

        if (!isGrounded && (!isStuckToWall_L && !isStuckToWall_R))
        {
            if (CanSlideLeft() && horizontalInput < 0)
                StickToWall(-1);
            else if (CanSlideRight() && horizontalInput > 0)
                StickToWall(1);
        }

        if (Input.GetButtonDown("Jump") /*&& (isGrounded || IsUpAgainstWallLeft() || IsUpAgainstWallRight())*/ && jumpCounter < maxJumpCount - 1 && !isDashing)
            Jump();
        else if (Input.GetButtonUp("Jump"))
            EndJump();
        if (!isDashing && dashTimer == 0 && midairDashCounter < maxMidairDashesCount)
        {
            if (Input.GetButtonDown("DashLeft"))
                DashLeft();
            if (Input.GetButtonDown("DashRight"))
                DashRight();
        }
        else if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                dashTimer = 0;
        }
        if (attackComboCount < 3 && !isDashing)
        {
            if (Input.GetButtonDown("Attack")) {
                if (isGrounded)
                    if (verticalInput >= 0)
                        Attack();
                    else
                        DownwardAttack();
                else
                    if (verticalInput >= 0)
                        AirAttack();
                    else
                        DownwardAttack();
            }
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
        if (isDashing)
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
            }else
                rb.gravityScale = Mathf.MoveTowards(rb.gravityScale, gravityScale * stuckToWall_g_mult, Time.fixedDeltaTime);

            if ((CanSlideLeft() && horizontalInput > 0) || (CanSlideRight() && horizontalInput < 0) || (!CanSlideLeft() && !CanSlideRight()))
                UnstickFromWall();
        }
        else if (isAttacking)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), acceleration);
        }
        else
        {
            if (horizontalInput == 0 || (horizontalInput > 0 && CanGoRight()) || (horizontalInput < 0 && CanGoLeft()))
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(horizontalInput * movSpeed, rb.velocity.y), acceleration);
            else
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), acceleration);
        }
        if(acceleration < accel)
            acceleration = Mathf.MoveTowards(acceleration, accel, Time.fixedDeltaTime);
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        isAttacking = true;
        attackComboCount++;
    }
    //called in animator events
    public void EndAttack()
    {
        isAttacking = false;
        attackComboCount = 0;
    }
    void AirAttack()
    {
        animator.SetTrigger("Attack");
        isAttacking = true;
        rb.velocity = new Vector2(rb.velocity.x, inAirAttackUpwardVel);
        attackComboCount++;
    }
    void DownwardAttack()
    {
        animator.SetTrigger("Downward Attack");
        isAttacking = true;
        rb.velocity = new Vector2(rb.velocity.x, downwardAttackUpwardVel);
    }

    void DashLeft()
    {
        SetDir(false);
        Dash(-1);
    }
    void DashRight()
    {
        SetDir(true);
        Dash(1);
    }
    void Dash(int direction)
    {
        Vector2 origin = coll.bounds.center;
        int numOfRays = 5;
        float yIncrament = coll.size.y / numOfRays;
        origin.y -= coll.size.y / 2f;

        Vector2 defaultPos = Vector3.right * direction * dashDistance + transform.position;
        dashPos = defaultPos;
        for (int i = 0; i < numOfRays + 1; i++)
        {
            Debug.DrawRay(origin, Vector2.right * direction * dashDistance, Color.white, 0.15f);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, dashDistance, unDashableMask);
            Vector2 hitPos = defaultPos;
            if (hit)
            {
                hitPos = new Vector2(hit.point.x - (direction * coll.size.x / 2f), transform.position.y);
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
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        animator.SetBool("Dashing", isGrounded);
        animator.SetBool("Rolling", !isGrounded);
    }
    void StopDashing()
    {
        dashTimer = minDelayBetweenDashes;
        isDashing = false;
        rb.gravityScale = gravityScale;

        animator.SetBool("Dashing", false);
        animator.SetBool("Rolling", false);
    }

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
    }

    void StickToWall(int direction)
    {
        isStuckToWall_L = direction == -1 ? true : false;
        isStuckToWall_R = !isStuckToWall_L;

        Vector2 origin = transform.position;
        Debug.DrawRay(origin, Vector2.right * direction * xRaylength, Color.yellow, 0.075f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, wallStickMask);
        if(hit)
            transform.position = hit.point + (Vector2.right * coll.size.x / 2f * -direction);

        jumpCounter = 0;
        midairDashCounter = 0;
    }
    void UnstickFromWall()
    {
        isStuckToWall_L = false;
        isStuckToWall_R = false;
        hasSlowedDownFromSticking = false;
        rb.gravityScale = gravityScale;
    }

    void SetDir(bool isRight)
    {
        spriteRenderer.flipX = !isRight;
        isDirRight = isRight;
    }

    bool IsGrounded()
    {
        Vector2 origin = coll.bounds.center;
        float xOffset = coll.bounds.size.x / 3f;
        origin.x = coll.bounds.min.x + xOffset / 2f;
        for (int i = 0; i < 3; i++)
        {
            Debug.DrawRay(origin, -Vector2.up * yRaylength, Color.blue, 0.075f);
            if (Physics2D.Raycast(origin, -Vector2.up, yRaylength, groundMask))
                return true;
            origin.x += xOffset;
        }
        return false;
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
        Vector2 origin = coll.bounds.center;
        if (perpRayCount == 1)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRaylength, debugColor, 0.075f);
            return Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, mask);
        }
        float yOffset = coll.size.y / perpRayCount;
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
        Vector2 origin = coll.bounds.center;
        if (perpRayCount == 1)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRaylength, debugColor, 0.075f);
            return Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, mask);
        }

        float yOffset = coll.size.y / perpRayCount;
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
