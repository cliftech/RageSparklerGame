using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private LootTable allDrops;



    public void Spawn(bool isDirRight)
    {
        int chance;
        Vector3 dropPlace;
        

        for (int i = 0; i < allDrops.GetComponent<LootTable>().lootTable.Length; ++i)
        {
            if (!isDirRight)
                dropPlace = new Vector3(Random.Range(0.00f, 2.00f), 1);
            else
                dropPlace = new Vector3(Random.Range(-2.00f, 0.00f), 1);    
            chance = Random.Range(0, 100);
            if (chance < allDrops.GetComponent<LootTable>().lootTable[i].DropChance)
            Instantiate(allDrops.GetComponent<LootTable>().lootTable[i].Item, this.transform.position + dropPlace, Quaternion.identity);
        }    
    }
}