using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float movSpeed = 5;
    public float acceleration = 5;
    public float jumpVel = 5;
    public float dashDistance = 3;
    public float dashSpeed = 20;
    public float stuckToWall_g_mult = 0.75f;

    public LayerMask groundMask;

    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private float horizontalInput;
    [Header("states (for debuging)")]
    [SerializeField]private bool isGrounded;
    [SerializeField] private bool isDashing;
    [SerializeField] private bool isStuckToWall;
    private Vector3 dashPos;
    private float gravityScale;
    private float accel;

    private float yRaylength;
    private float xRaylength;

    void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
	}
    void Start()
    {
        yRaylength = coll.size.y / 2 + 0.05f;
        xRaylength = coll.size.x / 2 + 0.05f;
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
        if (!isGrounded && !isStuckToWall)
            if ((IsUpAgainstWallLeft() && horizontalInput < 0) || (IsUpAgainstWallRight() && horizontalInput > 0))
                StickToWall();

        if ((isGrounded || isStuckToWall) && !isDashing && Input.GetButtonDown("Jump"))
            Jump();
        else if (Input.GetButtonUp("Jump"))
            EndJump();
        if (Input.GetButtonDown("DashLeft"))
            DashLeft();
        if (Input.GetButtonDown("DashRight"))
            DashRight();
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
        else if (isStuckToWall)
        {
            rb.gravityScale = Mathf.MoveTowards(rb.gravityScale, gravityScale * stuckToWall_g_mult, Time.fixedDeltaTime * 2f);

            if ((IsUpAgainstWallLeft() && horizontalInput > 0) || (IsUpAgainstWallRight() && horizontalInput < 0))
            {
                isStuckToWall = false;
                rb.gravityScale = gravityScale;
            }
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(horizontalInput * movSpeed, rb.velocity.y), acceleration);
        }
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

        isDashing = true;
        isStuckToWall = false;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }
    void StopDashing()
    {
        isDashing = false;
        rb.gravityScale = gravityScale;
    }

    void Jump()
    {
        Vector3 jumpDir;
        if (isStuckToWall)
        {
            jumpDir = IsUpAgainstWallLeft() ? new Vector2(1, 1) : new Vector2(-1, 1);
            acceleration = 0;
        }
        else
            jumpDir = Vector2.up;
        jumpDir.Normalize();

        rb.velocity = jumpDir * jumpVel;
        isStuckToWall = false;
        rb.gravityScale = gravityScale;
    }
    void EndJump()
    {
        if(rb.velocity.y > 0)
            rb.AddForce(-Vector2.up * jumpVel / 2.5f);
    }
    void Land()
    {
        StopDashing();
        isStuckToWall = false;
        rb.gravityScale = gravityScale;
    }

    void StickToWall()
    {
        isStuckToWall = true;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }

    bool IsGrounded()
    {
        Vector2 origin = coll.bounds.center;
        Debug.DrawRay(origin, -Vector2.up * yRaylength, Color.blue, 0.075f);
        return Physics2D.Raycast(origin, -Vector2.up, yRaylength, groundMask);
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
        Debug.DrawRay(origin, Vector2.right * direction * xRaylength, Color.blue, 0.075f);
        return Physics2D.Raycast(origin, Vector2.right * direction, xRaylength, groundMask);
    }

    float GetHorizontalDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x);
    }
}
