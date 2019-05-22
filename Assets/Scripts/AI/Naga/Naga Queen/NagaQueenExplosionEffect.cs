using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NagaQueenExplosionEffect : MonoBehaviour
{
    public float activeTime;
    private Animator animator;
    private DamageContainer damageContainer;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    public void Set(Vector3 position, float damage)
    {
        damageContainer = GetComponent<DamageContainer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();
        circleCollider.enabled = false;
        damageContainer.SetDamageCall(() => damage);
        DisableCollider();
    }
    public void EnableCollider()
    {
        circleCollider.enabled = true;
        StartCoroutine(EndExplosion(activeTime));
    }
    public void DisableCollider()
    {
        circleCollider.enabled = false;
        Destroy(gameObject, 3);
    }
    IEnumerator EndExplosion(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        animator.SetTrigger("End");
    }
}
