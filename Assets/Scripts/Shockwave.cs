using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    private ParticleSystem trail, shockwave;
    private LayerMask terrainMask;
    private DamageContainer damageContainer;
    new private Collider2D collider;

    private float speed;
    private float timer;
    private Vector3 direction;

    public AudioClip soundLoop;
    private AudioSource audioSource;

    public void Set(Vector2 position, float time, Vector2 direction, float damage, float speed = 10)
    {
        audioSource = GetComponent<AudioSource>();
        trail = GetComponent<ParticleSystem>();
        shockwave = GetComponentInChildren<ParticleSystem>();
        damageContainer = GetComponent<DamageContainer>();
        collider = GetComponentInChildren<Collider2D>();
        terrainMask = 1 << LayerMask.NameToLayer("Terrain");
        damageContainer.SetDamageCall(() => damage);

        this.speed = speed;
        this.timer = time;
        this.direction = direction;
        transform.position = position;

        RaycastHit2D hit = GetGroundHit();
        if (hit)
            transform.position = hit.point;

        trail.Play();
        shockwave.Play();
        audioSource.clip = soundLoop;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        transform.position += direction * Time.deltaTime * speed;
        if (timer <= 0 || !GetGroundHit() || GetWallHit())
        {
            trail.Stop();
            shockwave.Stop();
            audioSource.Stop();
            this.enabled = false;
            collider.enabled = false;
            Destroy(gameObject, 1f);
        }
    }

    RaycastHit2D GetGroundHit()
    {
        Debug.DrawRay(transform.position, Vector2.down * 0.5f, Color.red);
        return Physics2D.Raycast(transform.position, Vector2.down, 0.5f, terrainMask);
    }

    RaycastHit2D GetWallHit()
    {
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, direction * 0.5f, Color.red);
        return Physics2D.Raycast(transform.position + Vector3.up * 0.1f, direction, 0.1f, terrainMask);
    }
}
