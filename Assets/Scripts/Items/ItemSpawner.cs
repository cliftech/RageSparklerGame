using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private LootTable allDrops;

    public void Spawn()
    {
        int chance;
        // Spawn that item 
        for (int i = 0; i < allDrops.GetComponent<LootTable>().lootTable.Length; ++i)
        {
            chance = Random.Range(0, 100);
            if (chance < allDrops.GetComponent<LootTable>().lootTable[i].DropChance)
            Instantiate(allDrops.GetComponent<LootTable>().lootTable[i].Item, this.transform.position, Quaternion.identity);
        }    
    }
}