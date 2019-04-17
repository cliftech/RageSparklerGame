using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NagaQueenExplosionEffect : MonoBehaviour
{
    public float fadeInTime;
    public float activeTime;
    public float fadeOutTime;
    private DamageContainer damageContainer;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    public void Set(Vector3 position, float damage)
    {
        damageContainer = GetComponent<DamageContainer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.enabled = false;
        damageContainer.SetDamageCall(() => damage);
        StartCoroutine(FadeIn());
    }
    private IEnumerator FadeIn()
    {
        float speed = 1 / fadeInTime;
        Color c = spriteRenderer.color;
        c.a = 0;
        spriteRenderer.color = c;
        while (c.a < 0.99f)
        {
            c.a += Time.deltaTime * speed;
            spriteRenderer.color = c;
            yield return null;
        }
        c.a = 1;
        spriteRenderer.color = c;
        circleCollider.enabled = true;
        yield return new WaitForSecondsRealtime(activeTime);
        circleCollider.enabled = false;
        speed = 1 / fadeOutTime;
        while (c.a > 0.01f)
        {
            c.a -= Time.deltaTime * speed;
            spriteRenderer.color = c;
            yield return null;
        }
        c.a = 0;
        spriteRenderer.color = c;
        Destroy(gameObject, 1);
    }
}
