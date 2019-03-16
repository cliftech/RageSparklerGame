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
        slot = new GameObject[totalSlots];
        for (int i = 0; i < totalSlots; i++)
        {
            slot[i] = InvSlots.transform.GetChild(i).gameObject;

            if (slot[i].GetComponent<Slot>().item == null)
                slot[i].GetComponent<Slot>().empty = true;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I) && inventoryUI.name == "EquipmentUI")
        {
            inventoryEnabled = !inventoryEnabled;
            if (inventoryEnabled == true)
                inventoryUI.SetActive(true);
            else
                inventoryUI.SetActive(false);
        }
    }

    public bool Equip(GameObject itemObj, int itemID, string itemType, string itemDescription, Sprite itemIcon)
    {
        if (itemType == "Helmet")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 0);
        else if (itemType == "Amulet")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 1);
        else if (itemType == "BodyArmor")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 2);
        else if (itemType == "Weapon")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 3);
        else if (itemType == "LegArmor")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 4);
        else if (itemType == "Boots")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 5);
        else if (itemType == "Gloves")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 6);
        else if (itemType == "SecondaryWeapon")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 7);
        else if (itemType == "Potions")
            return add(itemObj, itemID, itemType, itemDescription, itemIcon, 8);
        else
            return false;
    }

    public bool add(GameObject itemObj, int itemID, string itemType, string itemDescription, Sprite itemIcon, int i)
    {
        if (slot[i].GetComponent<Slot>().empty)
        {
            itemObj.GetComponent<Item>().pickedUp = true;

            slot[i].GetComponent<Slot>().icon = itemIcon;
            slot[i].GetComponent<Slot>().type = itemType;
            slot[i].GetComponent<Slot>().description = itemDescription;
            slot[i].GetComponent<Slot>().ID = itemID;
            slot[i].GetComponent<Slot>().item = itemObj;

            itemObj.transform.parent = slot[i].transform;
            itemObj.SetActive(false);

            slot[i].GetComponent<Slot>().UpdateSlot();
            slot[i].GetComponent<Slot>().empty = false;
            return true;
        }
        return false;
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
                slot[i].GetComponent<Slot>().description = itemDescription;
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

    public GameObject GetPotion()
    {
        return slot[8].GetComponent<Slot>().item;
    }
    public void RemoveItem(string type)
    {
        for (int i = 0; i < slot.Length; i++)
        {
            if (slot[i].GetComponent<Slot>().type == type)
            {
                Remove(i);
                break;
            }
        }
    }
    public void RemoveItem(int id)
    {
        for (int i = 0; i < slot.Length; i++)
        {
            if (slot[i].GetComponent<Slot>().ID == id)
            {
                Remove(i);
                break;
            }
        }
    }
    void Remove(int i)
    {
        slot[i].GetComponent<Slot>().icon = default;
        slot[i].GetComponent<Slot>().type = "";
        slot[i].GetComponent<Slot>().description = "";
        slot[i].GetComponent<Slot>().ID = 0;
        slot[i].GetComponent<Slot>().item = null;
        slot[i].GetComponent<Slot>().UpdateSlot();
        slot[i].GetComponent<Slot>().empty = true;
    }
    public GameObject FindItemByType(string itemType)
    {
        for (int i = 0; i < slot.Length; i++)
        {
            if (slot[i] != null)
            {
                if (slot[i].name == itemType)
                {
                    return slot[i];
                }
            }
        }
        return null;
    }
}
