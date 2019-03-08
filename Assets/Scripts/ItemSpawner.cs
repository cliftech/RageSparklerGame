using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private int minimumCount = 1;
    [SerializeField]
    private int maximumCount = 10;
    [SerializeField]
    private GameObject prefab = null;
    [SerializeField]
    private int dropChance = 100;

    public int MinimumCount
    {
        get { return this.minimumCount; }
        set { this.minimumCount = value; }
    }
    public int MaximumCount
    {
        get { return this.maximumCount; }
        set { this.maximumCount = value; }
    }
    public GameObject Prefab
    {
        get { return this.prefab; }
        set { this.prefab = value; }
    }

    public void Spawn()
    {
        int rollDrop = Random.Range(0, 100);
        // Randomly pick the count of item to spawn.
        int count = Random.Range(this.MinimumCount, this.MaximumCount);
        // Spawn that item 
        if(rollDrop <= dropChance)
        {
            for (int i = 0; i < count; ++i)
            {
                Instantiate(this.prefab, this.transform.position, Quaternion.identity);
            }
        }      
    }
}