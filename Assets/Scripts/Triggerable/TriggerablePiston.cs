using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerablePiston : TriggerableTrap
{
    public enum Direction { Horizontal, Vertical };

    private ParticleSystem smashGroundParticles;
    private AudioSource audioSource;
    private CameraController cameraController;
    private Transform piston;
    private Collider2D pistonCollider;
    private Rigidbody2D pistonRb;
    private Vector2 retractedPosision;
    private Vector2 extendedPosision;

    public bool isRetracted = true;
    public Direction extendingDirection;
    public float extendDistance = 3;
    public float extendSpeed = 15f;
    public float retractSpeed = 7f;
    public float extendedStayTime = 1f;

    [Header("only important if not triggered by pressure plate")]
    public bool isTriggeredByPressurePlate = true;
    public float retractedStayTime = 1f;
    public float startDelay = 0;

    public AudioClip smashGroundSound;

    LayerMask terrainMask;

    private bool canSquish = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        cameraController = FindObjectOfType<CameraController>();
        terrainMask = LayerMask.GetMask("Terrain");
        piston = transform.Find("Piston");
        smashGroundParticles = piston.transform.Find("GroundSmashParticles").GetComponent<ParticleSystem>();

        pistonCollider = piston.GetComponent<Collider2D>();
        pistonRb = piston.GetComponent<Rigidbody2D>();

        Vector2 extendedOffset = extendingDirection == Direction.Horizontal ?
                new Vector2(extendDistance, 0) : new Vector2(0, extendDistance);
        if (isRetracted)
        {
            retractedPosision = piston.position;
            extendedPosision = retractedPosision + extendedOffset;
        }
        else
        {
            extendedPosision = piston.position;
            retractedPosision = extendedPosision - extendedOffset;
            isRetracted = true;
            piston.position = retractedPosision;
        }

        if (!isTriggeredByPressurePlate)
            StartCoroutine(Wait(ExtendRoutine(), startDelay));
    }

    public override void Trigger()
    {
        if(isRetracted)
            StartCoroutine(ExtendRoutine());
    }

    private IEnumerator ExtendRoutine()
    {
        canSquish = true;
        float distanceToTarget = Vector2.Distance(piston.position, extendedPosision);
        while (distanceToTarget > 0.05f)
        {
            distanceToTarget = Vector2.Distance(piston.position, extendedPosision);
            pistonRb.MovePosition( Vector2.MoveTowards(pistonRb.position, extendedPosision, Time.deltaTime * extendSpeed) );
            yield return null;
        }
        cameraController.Shake(3, 0.1f);
        audioSource.PlayOneShot(smashGroundSound);
        smashGroundParticles.Play();
        piston.position = extendedPosision;
        isRetracted = false;
        canSquish = false;
        StartCoroutine(Wait(RetractRoutine(), extendedStayTime));
    }

    private IEnumerator Wait(IEnumerator routineToStartAfter, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        StartCoroutine(routineToStartAfter);
    }

    private IEnumerator RetractRoutine()
    {
        canSquish = true;
        while (Vector2.Distance(piston.position, retractedPosision) > 0.05f)
        {
            pistonRb.MovePosition( Vector2.MoveTowards(pistonRb.position, retractedPosision, Time.deltaTime * retractSpeed) );
            yield return null;
        }
        piston.position = retractedPosision;
        isRetracted = true;

        if (!isTriggeredByPressurePlate)
            StartCoroutine(Wait(ExtendRoutine(), retractedStayTime));
        canSquish = false;
    }

    private IEnumerator TurnOffCollisionWithPlayer(float time, Collider2D playerCollider)
    {
        Physics2D.IgnoreCollision(pistonCollider, playerCollider, true);
        yield return new WaitForSecondsRealtime(time);
        Physics2D.IgnoreCollision(pistonCollider, playerCollider, false);
    }

    public void OnCollisionStay2D_Triggered (Collision2D collision)
    {
        if (canSquish && collision.collider.CompareTag("Player"))
        {
            Bounds b = collision.collider.bounds;

            Vector2 castDir;
            Vector2 castOrigin;
            float rayLength = 0.1f;
            if (extendingDirection == Direction.Horizontal)
            {
                if (collision.collider.bounds.center.x < pistonCollider.bounds.center.x)
                {
                    castOrigin = new Vector3(b.min.x, b.center.y);
                    castDir = Vector2.left;
                }
                else
                {
                    castOrigin = new Vector3(b.max.x, b.center.y);
                    castDir = Vector2.right;
                }
            }
            else
            {
                if (collision.collider.bounds.center.y < pistonCollider.bounds.center.y)
                {
                    castOrigin = new Vector3(b.center.x, b.min.y);
                    castDir = Vector2.down;
                }
                else
                {
                    castOrigin = new Vector3(b.center.x, b.max.y);
                    castDir = Vector2.up;
                }
            }

            Debug.DrawRay(castOrigin, castDir * rayLength, Color.red, 0.25f);
            if (Physics2D.Raycast(castOrigin, castDir, rayLength, terrainMask))
            {
                collision.collider.GetComponent<Player>().Squish();
                StartCoroutine(TurnOffCollisionWithPlayer(retractedStayTime + 0.1f, collision.collider));
            }
        }
    }
}
