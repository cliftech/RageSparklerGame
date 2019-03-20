using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private LootTable allDrops;

    public Item itm;



    void Start()
    {
        itm = GetComponent<Item>();
    }

    public void Spawn()
    {
        int chance;
        float dropDirection;
        

        for (int i = 0; i < allDrops.GetComponent<LootTable>().lootTable.Length; ++i)
        {  
            dropDirection = Random.Range(-5.0f, 5.0f);
            chance = Random.Range(0, 100);
            if (chance < allDrops.GetComponent<LootTable>().lootTable[i].DropChance)
            {
                itm = Instantiate(allDrops.GetComponent<LootTable>().lootTable[i].Item, this.transform.position, Quaternion.identity);
                itm.GetComponent<Rigidbody2D>().velocity = new Vector2(dropDirection, 2.0f);
            }
        }    
    }
}