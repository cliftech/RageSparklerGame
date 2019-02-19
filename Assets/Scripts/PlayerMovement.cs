using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float movSpeed = 5;
    public float acceleration = 5;
    public float jumpForce = 500;
    public float dashDistance = 3;
    public float dashSpeed = 20;

    public LayerMask groundMask;

    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private float horizontalInput;
    [SerializeField]private bool isGrounded;
    [SerializeField] private bool isDashing;
    private Vector3 dashPos;
    private float gravityScale;

    private float groundedRayLength;
	void Awake ()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
	}
    void Start()
    {
        groundedRayLength = coll.size.y / 2 + 0.05f;
        gravityScale = rb.gravityScale;
    }
	
	void Update ()
    {
        bool newGrounded = IsGrounded();
        if (!isGrounded && newGrounded)
            Land();
        isGrounded = newGrounded;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (isGrounded && !isDashing && Input.GetButtonDown("Jump"))
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
        else
        {
             rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(horizontalInput * movSpeed, rb.velocity.y), acceleration);
        }
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
        rb.AddForce(Vector2.up * jumpForce);
    }
    void EndJump()
    {
        print(rb.velocity);
        if(rb.velocity.y > 0)
            rb.AddForce(-Vector2.up * jumpForce / 2.5f);
    }
    void Land()
    {
        StopDashing();
    }

    bool IsGrounded()
    {
        Vector2 origin = coll.bounds.center;
        Debug.DrawRay(origin, -Vector2.up * groundedRayLength, Color.blue, 0.075f);
        return Physics2D.Raycast(origin, -Vector2.up, groundedRayLength, groundMask);
    }

    float GetHorizontalDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x);
    }
}
