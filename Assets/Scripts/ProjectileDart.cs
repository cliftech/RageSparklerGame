using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDart : MonoBehaviour
{
    new private Collider2D collider;
    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    public AudioClip stickInWallClip;
    public AudioClip stickInFleshClip;

    private float speed;
    private Vector3 direction;
    private float damage;

    public void Set(Vector2 direction, float speed, float damage)
    {
        collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        this.speed = speed;
        this.direction = direction;
        this.damage = damage;
        GetComponent<DamageContainer>().SetDamageCall(() => damage);

        float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 180);
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, transform.position + direction, Time.deltaTime * speed);

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") /*|| other.CompareTag("Enemy")*/)
        {
            audioSource.clip = stickInFleshClip;
            audioSource.Play();

            rb.isKinematic = false;
            rb.gravityScale = 1;
            collider.isTrigger = false;
            gameObject.layer = LayerMask.NameToLayer("NonKinematicNonInteractable");
            this.enabled = false;
        }
        else if(!other.CompareTag("Enemy"))
        {
            audioSource.clip = stickInWallClip;
            audioSource.Play();

            collider.enabled = false;
            animator.SetTrigger("StickInWall");
            this.enabled = false;
        }
    }
}
