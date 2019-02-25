using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float base_maxhealth = 100;
    public float health_perLevel = 20;
    public float Health { get { return health; } }
    public int Level { get { return level; } }
    private float health;
    private int level;

    void Start()
    {
        Initialize();
    }

    public void Hit(float damage)
    {
        health -= damage;
        print("PC Health - " + health);
    }

    private void Initialize()
    {
        health = base_maxhealth + level * health_perLevel;
    }

    public float GetDamage()
    {
        return 10;
    }
}
