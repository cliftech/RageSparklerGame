using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummy : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    DamageContainer damageContainer;
    private Player player;
    public float maxHealth = 100;
    private float health = 100;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageContainer = GetComponent<DamageContainer>();
    }

    void Start()
    {
        health = maxHealth;
        damageContainer.SetDamageCall(() => 0);
    }

    void GetHit()
    {
        health -= player.GetDamage();
        spriteRenderer.color = new Color((health / maxHealth), 0.1f, 0.1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "PlayerWeapon")
        {
            GetHit();
        }
    }
}
