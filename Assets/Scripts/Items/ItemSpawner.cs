using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private LootTable allDrops;

    public Item itm;

    private GameObject spawnObject;

    public void Spawn(bool directionLeft)
    {
        int chance;
        float dropDirection;
        

        for (int i = 0; i < allDrops.GetComponent<LootTable>().lootTable.Length; ++i)
        {  
            if(directionLeft)
             dropDirection = Random.Range(-3.0f, -1.0f);
            else dropDirection = Random.Range(1.0f, 3.0f);
            chance = Random.Range(0, 100);
            if (chance < allDrops.GetComponent<LootTable>().lootTable[i].DropChance)
            {
                spawnObject = Instantiate(allDrops.GetComponent<LootTable>().lootTable[i].Item.Object2D, this.transform.position, Quaternion.identity);
                spawnObject.GetComponent<SpriteRenderer>().sprite = allDrops.GetComponent<LootTable>().lootTable[i].Item.icon;
                spawnObject.GetComponent<PickUp>().item = allDrops.GetComponent<LootTable>().lootTable[i].Item;
                spawnObject.GetComponent<Rigidbody2D>().velocity = new Vector2(dropDirection, 2.0f);
            }
        }
        for (int i = 0; i < allDrops.GetComponent<LootTable>().essence.Length; ++i)
        {
            if (directionLeft)
                dropDirection = Random.Range(-3.0f, -1.0f);
            else dropDirection = Random.Range(1.0f, 3.0f);

            spawnObject = Instantiate(allDrops.GetComponent<LootTable>().essence[i], this.transform.position, Quaternion.identity);
            spawnObject.GetComponent<Rigidbody2D>().velocity = new Vector2(dropDirection, 2.0f);
        }
    }
}