using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public List<GameObject> items;
    private List<ItemPlaceholder> itemPlaceholders;

    public static ItemDatabase instance;

    private void Awake()
    {
        instance = this;
        itemPlaceholders = new List<ItemPlaceholder>();
        foreach (var i in items)
            itemPlaceholders.Add(new ItemPlaceholder(i.GetComponent<Item>().ID, i));
    }

    public GameObject GetItemByID(int id)
    {
        foreach (var i in itemPlaceholders)
            if (i.id == id)
                return i.item;
        Debug.LogWarning("Item with id: " + id + " not found");
        return null;
    }

    private class ItemPlaceholder
    {
        public int id;
        public GameObject item;

        public ItemPlaceholder(int id, GameObject item)
        {
            this.id = id;
            this.item = item;
        }
    }
}


