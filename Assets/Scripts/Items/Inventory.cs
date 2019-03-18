﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private static GameObject toolTip;
    private static Text textBox;
    private static Text visualText;
    public GameObject toolTipObject;    
    public Text textBoxObject;
    public Text visualTextObject;

    public GameObject inventoryUI;
    public GameObject InvSlots;
    public bool inventoryEnabled;

    public GameObject[] slot;
    public int totalSlots;

    public float slotPaddingHorizontal;
    public float slotPaddingVertical;

    void Start()
    {
        visualText = visualTextObject;
        textBox = textBoxObject;
        toolTip = toolTipObject;
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
            toolTipObject.SetActive(false);
        }
    }

    public void ShowToolTip(GameObject slot)
    {
        
        Slot tmpslot = slot.GetComponent<Slot>();
        if (!tmpslot.empty)
        {
            visualText.text = tmpslot.GetToolTip();
            textBox.text = visualText.text;

            toolTip.SetActive(true);

            float xPos = slot.transform.position.x - slotPaddingHorizontal - 35;
            float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - slotPaddingVertical - 15;

            toolTip.transform.position = new Vector2(xPos, yPos);
        }
    }
    public void HideToolTip()
    {
        toolTip.SetActive(false);
    }

    public bool Equip(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemName, Sprite itemIcon, string quality)
    {
        if (itemType == "Helmet")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 0);
        else if (itemType == "Amulet")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 1);
        else if (itemType == "BodyArmor")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 2);
        else if (itemType == "Weapon")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 3);
        else if (itemType == "LegArmor")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 4);
        else if (itemType == "Boots")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 5);
        else if (itemType == "Gloves")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 6);
        else if (itemType == "SecondaryWeapon")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 7);
        else if (itemType == "Potions")
            return add(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 8);
        else
            return false;
    }

    public bool add(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemName, Sprite itemIcon, string quality, int i)
    {
        if (slot[i].GetComponent<Slot>().empty)
        {
            itemObj.GetComponent<Item>().pickedUp = true;

            slot[i].GetComponent<Slot>().icon = itemIcon;
            slot[i].GetComponent<Slot>().type = itemType;
            slot[i].GetComponent<Slot>().description = itemDescription;
            slot[i].GetComponent<Slot>().ID = itemID;
            slot[i].GetComponent<Slot>().item = itemObj;
            slot[i].GetComponent<Slot>().quality = quality;
            slot[i].GetComponent<Slot>().itemName = itemName;

            itemObj.transform.parent = slot[i].transform;
            itemObj.SetActive(false);

            slot[i].GetComponent<Slot>().UpdateSlot();
            slot[i].GetComponent<Slot>().empty = false;
            return true;
        }
        return false;
    }

    public void AddItem(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemName, string quality, Sprite itemIcon)
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
                slot[i].GetComponent<Slot>().itemName = itemName;
                slot[i].GetComponent<Slot>().quality = quality;

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
        slot[i].GetComponent<Slot>().quality = "";
        slot[i].GetComponent<Slot>().itemName = "";
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