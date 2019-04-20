using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public List<Item> items;
    private List<ItemPlaceholder> itemPlaceholders;

    public static ItemDatabase instance;

    private void Awake()
    {
        instance = this;
        itemPlaceholders = new List<ItemPlaceholder>();
        for (int i = 0; i < items.Count; i++)
        {
            items[i].ID = i.ToString();
            itemPlaceholders.Add(new ItemPlaceholder(items[i].ID, items[i]));
        }
        foreach (var i in items)
            itemPlaceholders.Add(new ItemPlaceholder(i.ID, i));
    }

    public Item GetItemByID(string id)
    {
        foreach (var i in itemPlaceholders)
            if (i.id == id)
                return i.item;
        Debug.LogWarning("Item with id: " + id + " not found");
        return null;
    }

    private class ItemPlaceholder
    {
        public string id;
        public Item item;

        public ItemPlaceholder(string id, Item item)
        {
            this.id = id;
            this.item = item;
        }
    }
}


