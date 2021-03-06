﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    new private SpriteRenderer renderer;
    private Animator animator;
    new private Collider2D collider;

    public float rotationSpeed = 2;
    public float degreeOffset = 180;
    public Color travelColor, explosionColor;

    private float speed, damage;
    private Vector2 targetPos;
    private Vector2 lookDirection;
    private bool explodeOnTargetDestination;
    private bool directionLocked;

    public void Set(Vector2 position, Vector2 lookDirection, string physicsLayerName, Vector2 targetPos, float speed, float damage, bool explodeOnTarget = false)
    {
        renderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
        renderer.color = travelColor;
        explodeOnTargetDestination = explodeOnTarget;

        transform.position = position;
        gameObject.layer = LayerMask.NameToLayer(physicsLayerName);
        this.targetPos = targetPos;
        this.speed = speed;
        this.damage = damage;
        this.lookDirection = lookDirection;

        GetComponent<DamageContainer>().SetDamageCall(() => damage);
    }

    void Update()
    {
        if(!directionLocked)
        {
            Vector2 targetDirection = (targetPos - (Vector2)transform.position);
            lookDirection = Vector2.Lerp(lookDirection, targetDirection, Time.deltaTime * rotationSpeed);

            Vector2 pos = transform.position;
            pos += lookDirection * Time.deltaTime * speed;
            transform.position = pos;

            float rot_z = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z - degreeOffset);

            if (Vector2.Distance(transform.position, targetPos) < 0.25f)
            {
                if(explodeOnTargetDestination)
                    Explode();
                else
                {
                    directionLocked = true;
                }
            }
        }
        else
        {
            Vector2 pos = transform.position;
            pos += lookDirection * Time.deltaTime * speed;
            transform.position = pos;
        }
    }

    public void Explode()
    {
        renderer.color = explosionColor;
        animator.SetTrigger("Explode");
        Destroy(gameObject, 5f);
        this.enabled = false;
        collider.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Explode();
    }
}
