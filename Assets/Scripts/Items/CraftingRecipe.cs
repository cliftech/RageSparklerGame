using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct ItemAmount
{
    public Item item;
    [Range(1,99)]
    public int amount;
}

[CreateAssetMenu]
public class CraftingRecipe : ScriptableObject
{
    public List<ItemAmount> Materials;
    public List<ItemAmount> Results;

    public bool CanCraft(Inventory inventory, Inventory hubchest)
    {
        int amount = 0;
        foreach(ItemAmount itemAmount in Materials)
        {
            amount = 0;
            amount += inventory.ItemCount(itemAmount.item);
            amount += hubchest.ItemCount(itemAmount.item);
            if(amount < itemAmount.amount)
            {
                return false;
            }
        }
        return true;
    }

    public void Craft(Inventory inventory, Inventory hubchest)
    {
        if (CanCraft(inventory, hubchest))
        {
            Item oldItem;
            foreach (ItemAmount itemAmount in Materials)
            {
                for (int i = 0; i < itemAmount.amount; i++)
                {
                    oldItem = hubchest.RemoveItemByID(itemAmount.item.ID);
                    if (oldItem == null)
                    {
                        inventory.RemoveItem(itemAmount.item);
                    }
                }
            }

            foreach (ItemAmount itemAmount in Results)
            {
                for (int i = 0; i < itemAmount.amount; i++)
                {
                    Item previousItem;
                    if (inventory.Equip(Instantiate(itemAmount.item), out previousItem) && previousItem != null)
                    {
                        hubchest.AddItemToHubChest(previousItem);
                    }
                }
            }
        }
    }
}
