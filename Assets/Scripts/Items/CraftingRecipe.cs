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
    [Header("[Add new recipes to: Canvas-CraftingWindow-CraftinWindow Component-CraftingRecipes !]")]
    public List<ItemAmount> Materials;
    public List<ItemAmount> Results;

    public bool CanCraft(Inventory inventory, Inventory hubchest)
    {
        foreach (ItemAmount itemAmount in Materials)
        {      
            if (CountAmount(inventory, hubchest, itemAmount) < itemAmount.amount)
            {
                return false;
            }
        }
        return true;
    }

    public int CountAmount(Inventory inventory, Inventory hubchest, ItemAmount itemAmount)
    {
        int amount = 0;
        amount += inventory.ItemCount(itemAmount.item);
        amount += hubchest.ItemCount(itemAmount.item);
        return amount;
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
                        inventory.RemoveItemByID(itemAmount.item.ID);
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
