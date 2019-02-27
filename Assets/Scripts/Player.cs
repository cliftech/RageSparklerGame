using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;

    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }
    public int Level { get { return level; } }
    private float health;
    private int level;
    public string enemyTag;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        Initialize();
    }

    public void GetHit(bool hitFromRightSide, float damage)
    {
        health -= damage;
        print("PC Health - " + health);
        playerMovement.KnockBack(hitFromRightSide, damage);
    }

    private void Initialize()
    {
        health = base_maxhealth + level * health_perLevel;
    }

    public float GetDamage()
    {
        return 10;
    }


    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.CompareTag(enemyTag))
            GetHit(coll.collider.transform.position.x > transform.position.x, 10);
    }
}
