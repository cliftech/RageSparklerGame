﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Base : MonoBehaviour
{
    protected Rigidbody2D rb;
    new protected SpriteRenderer renderer;
    protected CapsuleCollider2D coll;
    protected Animator animator;
    protected Transform target;
    protected DamageContainer damageContainer;
    protected Collider2D[] bodyWeaponColliders;
    protected ItemSpawner itemSpawner;

    public enum State { Patrol, Aggro, Attacking, KnockedBack, Dead, Idle, Awakening, Running };
    protected State state;

    protected bool isDirRight;
    protected float xRayLength;
    protected Vector2 originalScale;
    protected float health;
    protected float knockBackTimer;
    protected Color aliveColor;

    public float movVelocity;
    public float aggroRange;
    public float maxHealth;
    public float knockBackVelocity;
    public float knockBackTime;
    public Color deadColor;

    protected void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        damageContainer = GetComponent<DamageContainer>();
        target = FindObjectOfType<Player>().transform;
        List<Collider2D> colls = new List<Collider2D>();
        foreach (var c in transform.Find("DamagePCHitbox").GetComponents<Collider2D>())
            colls.Add(c);
        bodyWeaponColliders = colls.ToArray();
        itemSpawner = GetComponent<ItemSpawner>();

    }
    protected void SetIdle()
    {
        state = State.Idle;
    }
    protected void SetPatrol()
    {
        state = State.Patrol;
    }
    protected void SetAggro()
    {
        state = State.Aggro;
    }
    protected void SetAttack1()
    {
        state = State.Attacking;
        animator.SetTrigger("Attack1");
    }
    protected void SetAttack2()
    {
        state = State.Attacking;
        animator.SetTrigger("Attack2");
    }
    protected void SetAttack3()
    {
        state = State.Attacking;
        animator.SetTrigger("Attack3");
    }
    protected void EndAttack()
    {
        SetAggro();
    }
    protected void SetKnockedBack()
    {
        state = State.KnockedBack;
        knockBackTimer = knockBackTime;
    }
    protected void EndKnockedBack()
    {
        SetAggro();
        knockBackTimer = 0;
    }
    protected void SetDead()
    {
        state = State.Dead;
        health = 0;
        //rb.velocity = new Vector2(0, 0);
        renderer.color = deadColor;
        foreach (var c in bodyWeaponColliders)
            c.enabled = false;
        StartCoroutine(DeathRoutine(.5f));
        animator.SetBool("Dead", true);
        itemSpawner.Spawn();
    }
    protected IEnumerator DeathRoutine(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        rb.isKinematic = true;
        coll.enabled = false;
    }
    protected void SetAwakening()
    {
        state = State.Awakening;
        renderer.color = aliveColor;
        foreach (var c in bodyWeaponColliders)
            c.enabled = true;
        rb.isKinematic = false;
        coll.enabled = true;
        animator.SetBool("Dead", false);
    }
    protected void SetRunning()
    {
        state = State.Running;
    }
    protected void ChangeDirection(bool toRight)
    {
        float posOffset = coll.bounds.center.x - transform.position.x;
        if (toRight)
        {
            transform.localScale = new Vector2(-originalScale.x, transform.localScale.y);
        }
        else
        {
            transform.localScale = new Vector2(originalScale.x, transform.localScale.y);
        }
        transform.position = new Vector3(transform.position.x + posOffset, transform.position.y, transform.position.z);
        isDirRight = toRight;
    }
    protected void GetHit(bool isRight, float damage)
    {
        health -= damage;
        print(name + " Health: " + health);
        KnockBack(isRight, damage);
    }
    protected void KnockBack(bool knockbackToLeftSide, float forceMult)
    {
        ChangeDirection(knockbackToLeftSide);
        Vector2 forceDir = (knockbackToLeftSide ? new Vector2(-1, .5f) : new Vector2(1, .5f)).normalized;
        rb.velocity = forceDir * knockBackVelocity * Mathf.Clamp(forceMult / 10, 1, 1.5f);
        SetKnockedBack();

        animator.SetTrigger("GetHit");
    }
    protected bool RaycastToPlayer(bool isRight, float distance, string playerTag, LayerMask playerMask, LayerMask wallMask, Color debugColor)
    {
        int direction;
        if (isRight)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }
        Vector2 origin = coll.bounds.center;
        origin.y = coll.bounds.min.y + 0.2f;
        Debug.DrawRay(origin, Vector2.right * direction * distance, debugColor, 0.075f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, distance, playerMask | wallMask);
        if (!hit)
        {
            return false;
        }
        return hit.collider.CompareTag(playerTag);
    }
    protected bool RaycastSideways_OR(bool isRight, int perpRayCount, float distance, LayerMask mask, Color debugColor)
    {
        int direction;
        if (isRight)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }
        Vector2 origin = coll.bounds.center;
        origin.x += coll.bounds.extents.x * direction;
        if (perpRayCount == 1)
        {
            Debug.DrawRay(origin, Vector2.right * direction * distance, debugColor, 0.075f);
            return Physics2D.Raycast(origin, Vector2.right * direction, distance, mask);
        }
        float yOffset = coll.size.y / perpRayCount;
        origin.y -= yOffset * perpRayCount / 2;
        for (int i = 0; i < perpRayCount + 1; i++)
        {
            Debug.DrawRay(origin, Vector2.right * direction * distance, debugColor, 0.075f);
            if (Physics2D.Raycast(origin, Vector2.right * direction, distance, mask))
                return true;
            origin.y += yOffset;
        }
        return false;
    }
}
