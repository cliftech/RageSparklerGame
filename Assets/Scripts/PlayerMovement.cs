using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float movSpeed = 5;
    public float acceleration = 5;
    public float jumpVel = 5;
    public float inAirJumpVelMult = 0.75f;
    public float dashDistance = 3;
    public float dashSpeed = 20;
    public float stuckToWall_g_mult = 0.75f;
    public float endJumpMult = 0.75f;
    public float minDelayBetweenDashes = 0.5f;
    public int maxJumpCount = 3;
    public int maxMidairDashesCount = 1;

    public LayerMask groundMask;

    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private float horizontalInput;
    [Header("states (for debuging)")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isDashing;
    [SerializeField] private bool isStuckToWall_L;
    [SerializeField] private bool isStuckToWall_R;
    private Vector3 dashPos;
    private float gravityScale;
    private float accel;

    private float yRaylength;
    private float xRaylength;

    private float dashTimer;
    private int jumpCounter;
    [SerializeField]private int midairDashCounter;

    void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
	}
    void Start()
    {
        yRaylength = coll.size.y / 2 + 0.05f;
        xRaylength = coll.size.x / 2 + 0.1f;
        gravityScale = rb.gravityScale;
        accel = acceleration;
    }
	
	void Update ()
    {
        bool newGrounded = IsGrounded();
        if (!isGrounded && newGrounded)
            Land();
        isGrounded = newGrounded;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (!isGrounded && (!isStuckToWall_L && !isStuckToWall_R))
        {
            if (IsUpAgainstWallLeft() && horizontalInput < 0)
                StickToWall(-1);
            else if (IsUpAgainstWallRight() && horizontalInput > 0)
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
	}

    void FixedUpdate()
    {
        if (isDashing)
        {
            transform.position = Vector2.Lerp(transform.position, dashPos, dashSpeed * Time.fixedDeltaTime);
            if (GetHorizontalDistance(transform.position, dashPos) < 0.1f)
                StopDashing();
            Debug.DrawLine(transform.position, dashPos, Color.white, 0.075f);
        }
        else if (isStuckToWall_L || isStuckToWall_R)
        {
            rb.gravityScale = Mathf.MoveTowards(rb.gravityScale, gravityScale * stuckToWall_g_mult, Time.fixedDeltaTime);

            if ((IsUpAgainstWallLeft() && horizontalInput > 0) || (IsUpAgainstWallRight() && horizontalInput < 0) || (!IsUpAgainstWallLeft() && !IsUpAgainstWallRight()))
                UnstickFromWall();
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(horizontalInput * movSpeed, rb.velocity.y), acceleration);
        }
        if(acceleration < accel)
            acceleration = Mathf.MoveTowards(acceleration, accel, Time.fixedDeltaTime);
    }

    void DashLeft()
    {
        Dash(-1);
    }
    void DashRight()
    {
        Dash(1);
    }
    void Dash(int direction)
    {
        Vector2 origin = coll.bounds.center;
        origin.y -= coll.size.y / 2f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, dashDistance, groundMask);
        if (hit)
            dashPos = new Vector2(hit.point.x - (direction * coll.size.x / 2f), transform.position.y);
        else
            dashPos = Vector3.right * direction * dashDistance + transform.position;

        if (!isGrounded)
            midairDashCounter++;
        isDashing = true;
        isStuckToWall_L = false;
        isStuckToWall_R = false;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }
    void StopDashing()
    {
        dashTimer = minDelayBetweenDashes;
        isDashing = false;
        rb.gravityScale = gravityScale;
    }

    void Jump()
    {
        Vector3 jumpDir;
        if (isStuckToWall_L || isStuckToWall_R)
        {
            jumpDir = IsUpAgainstWallLeft() ? new Vector2(1, 1) : new Vector2(-1, 1);
            acceleration = 0;
        }
        else
            jumpDir = Vector2.up;
        jumpDir.Normalize();

        rb.velocity = jumpDir * (isGrounded || isStuckToWall_L || isStuckToWall_R ? jumpVel : jumpVel * inAirJumpVelMult);
        if(!isGrounded && !isStuckToWall_L && !isStuckToWall_R)
            jumpCounter++;
        UnstickFromWall();
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
    }

    void StickToWall(int direction)
    {
        isStuckToWall_L = direction == -1 ? true : false;
        isStuckToWall_R = !isStuckToWall_L;

        Vector2 origin = transform.position;
        Debug.DrawRay(origin, Vector2.right * direction * xRaylength, Color.yellow, 0.075f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, groundMask);
        if(hit)
            transform.position = hit.point + (Vector2.right * coll.size.x / 2f * -direction);

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        jumpCounter = 0;
        midairDashCounter = 0;
    }
    void UnstickFromWall()
    {
        isStuckToWall_L = false;
        isStuckToWall_R = false;
        rb.gravityScale = gravityScale;
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

    bool IsUpAgainstWallLeft()
    {
        return IsUpAgainstWall(-1);
    }
    bool IsUpAgainstWallRight()
    {
        return IsUpAgainstWall(1);
    }
    bool IsUpAgainstWall(int direction)
    {
        Vector2 origin = coll.bounds.center;
        float yOffset = coll.bounds.size.y / 4f;
        origin.y = coll.bounds.min.y + yOffset  * 1.5f;
        for (int i = 0; i < 2; i++)
        {
            Debug.DrawRay(origin, Vector2.right * direction * xRaylength, Color.blue, 0.075f);
            if (Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, groundMask))
                return true;
            origin.y += yOffset;
        }
        return false;
    }

    float GetHorizontalDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x);
    }
}
