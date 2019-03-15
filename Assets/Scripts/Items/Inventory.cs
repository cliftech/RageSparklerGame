using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryUI;
    public GameObject InvSlots;
    public bool inventoryEnabled;

    public GameObject[] slot;
    public int totalSlots;

    void Start()
    {
        totalSlots = 100;
        slot = new GameObject[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            slot[i] = InvSlots.transform.GetChild(i).gameObject;

            if (slot[i].GetComponent<Slot>().item == null)
                slot[i].GetComponent<Slot>().empty = true;
        }
    }
    public void AddItem(GameObject itemObj, int itemID, string itemType, string itemDescription, Sprite itemIcon)
    {
        for (int i = 0; i < totalSlots; i++)
        {
            if (slot[i].GetComponent<Slot>().empty)
            {
                itemObj.GetComponent<Item>().pickedUp = true;

                slot[i].GetComponent<Slot>().icon = itemIcon;
                slot[i].GetComponent<Slot>().type = itemType;
                slot[i].GetComponent<Slot>().descriptio = itemDescription;
                slot[i].GetComponent<Slot>().ID = itemID;
                slot[i].GetComponent<Slot>().item = itemObj;

                itemObj.transform.parent = slot[i].transform;
                itemObj.SetActive(false);

                slot[i].GetComponent<Slot>().UpdateSlot();
                slot[i].GetComponent<Slot>().empty = false;
                break;
            }      
        }
    }

    //public void RemoveItem(GameObject item)
    //{
    //    for (int i = 0; i < inventory.Length; i++)
    //    {
    //        if(inventory[i] == item)
    //        {
    //            inventory[i] = null;
    //            print(item.name + " used");
    //            break;
    //        }
    //    }
    //}
    //public GameObject FindItemByType(string itemType)
    //{
    //    for (int i = 0; i < inventory.Length; i++)
    //    {
    //        if (inventory[i] != null)
    //        {
    //            if (inventory[i].GetComponent<InteractionObject>().itemType == itemType)
    //            {
    //                return inventory[i];
    //            }
    //        }              
    //    }
    //    return null;
    //}
}
