using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private static GameManager gameManager;
    private static GameObject toolTip;
    private static GameObject compareToolTip;
    private static Text textBox;
    private static Text visualText;
    private static Text compareTextBox;
    private static Text compareVisualText;
    private static PlayerInteract plrInter;

    private Player player;

    private CameraController followCamera;

    public Slot slotC;

    public GameObject toolTipObject;
    public Text textBoxObject;
    public Text visualTextObject;
    public GameObject compareToolTipObject;
    public Text compareTextBoxObject;
    public Text compareVisualTextObject;
    public PlayerInteract plrInterObject;

    public GameObject inventoryUI;
    public GameObject InvSlots;
    public bool inventoryEnabled;

    public GameObject[] slot;
    public int totalSlots;
    public int invsOpen;

    public float slotPaddingHorizontal;
    public float slotPaddingVertical;

    void Awake()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    void Start()
    {
        visualText = visualTextObject;
        textBox = textBoxObject;
        toolTip = toolTipObject;
        compareToolTip = compareToolTipObject;
        compareTextBox = compareTextBoxObject;
        compareVisualText = compareVisualTextObject;
        slot = new GameObject[totalSlots];
        plrInter = plrInterObject;
        player = GetComponent<Player>();
        slotC = GetComponent<Slot>();
        followCamera = GameObject.Find("Main Camera").GetComponent<CameraController>();
        invsOpen = 0;

        for (int i = 0; i < totalSlots; i++)
        {
            slot[i] = InvSlots.transform.GetChild(i).gameObject;
            slot[i].GetComponent<Slot>().whichSlot = i;
            if (slot[i].GetComponent<Slot>().item == null)
                slot[i].GetComponent<Slot>().empty = true;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("OpenInv") && inventoryUI.name == "EquipmentUI" && !inventoryEnabled)
        {
            inventoryEnabled = true;
            EventSystem.current.SetSelectedGameObject(slot[0]);
            inventoryUI.SetActive(true);
            player.playerMovement.SetEnabled(false);
            followCamera.SetEnabled(false);
            toolTipObject.SetActive(false);
            compareToolTipObject.SetActive(false);
            ShowToolTip(slot[0]);
            invsOpen++;
        }
        else if (Input.GetButtonDown("OpenInv") && inventoryUI.name == "EquipmentUI" && inventoryEnabled)
        {
            inventoryUI.SetActive(false);
            invsOpen--;
            if (invsOpen == 0)
            {
                player.playerMovement.SetEnabled(true);
                followCamera.SetEnabled(true);
            }
            EventSystem.current.SetSelectedGameObject(plrInter.hubChest.slot[0]);
            inventoryEnabled = false;
            FindGrey();
            toolTipObject.SetActive(false);
            compareToolTipObject.SetActive(false);
            if (plrInter.hubChest.inventoryEnabled)
            {
                ShowToolTip(plrInter.hubChest.slot[0]);
                CompareToolTips(plrInter.hubChest.slot[0]);
            }
        }
    }

    public void ShowToolTip(GameObject slot)
    {
        Slot tmpslot = slot.GetComponent<Slot>();
        Slot equipped;
        Transform panel = tmpslot.transform.GetChild(0);
        tmpslot.selected = true;
        panel.GetComponent<Image>().color = Color.grey;

        if (!tmpslot.empty)
        {
            equipped = plrInter.equipment.FindItemByType(tmpslot.type).GetComponent<Slot>();
            if (!equipped.empty)
                tmpslot.CompareItems(equipped);
            visualText.text = tmpslot.GetToolTip(false);
            textBox.text = visualText.text;

            toolTip.SetActive(true);
            Canvas.ForceUpdateCanvases();
            float xPos = slot.transform.position.x - slotPaddingHorizontal - 35;
            float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - slotPaddingVertical - 15;
            toolTip.transform.position = new Vector2(xPos, yPos);
        }
    }

    public void CompareToolTips(GameObject slot)
    {
        plrInter.CompareToolTips(slot, compareVisualText, compareTextBox, compareToolTip, toolTip);
    }

    public void HideToolTip(GameObject slot)
    {
        Slot tmpslot = slot.GetComponent<Slot>();
        Transform panel = tmpslot.transform.GetChild(0);
        panel.GetComponent<Image>().color = Color.white;
        toolTip.SetActive(false);
        tmpslot.selected = false;
    }
    public void HideCompareToolTips()
    {
        plrInter.HideCompareToolTips(compareToolTip);
    }

    public bool Equip(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemName, Sprite itemIcon, string quality, float damage, float armor, float health)
    {
        if (itemType == "Helmet")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 0, damage, armor, health);
        else if (itemType == "Amulet")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 1, damage, armor, health);
        else if (itemType == "BodyArmor")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 2, damage, armor, health);
        else if (itemType == "Weapon")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 3, damage, armor, health);
        else if (itemType == "LegArmor")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 4, damage, armor, health);
        else if (itemType == "Boots")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 5, damage, armor, health);
        else if (itemType == "Gloves")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 6, damage, armor, health);
        else if (itemType == "SecondaryWeapon")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 7, damage, armor, health);
        else if (itemType == "Potions")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 8, damage, armor, health);
        else if (itemType == "Ring")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 9, damage, armor, health);
        else if (itemType == "Cape")
            return AddItemToPlayerInv(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, quality, 10, damage, armor, health);
        else
            return false;
    }

    public bool AddItemToPlayerInv(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemName, Sprite itemIcon, string quality, int i, float damage, float armor, float health)
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
            slot[i].GetComponent<Slot>().damage = damage;
            slot[i].GetComponent<Slot>().armor = armor;
            slot[i].GetComponent<Slot>().health = health;

            itemObj.transform.parent = slot[i].transform;
            itemObj.SetActive(false);

            slot[i].GetComponent<Slot>().UpdateSlot();
            slot[i].GetComponent<Slot>().empty = false;

            gameManager.SaveGame();
            return true;
        }
        return false;
    }

    public void AddItemToHubChest(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemName, string quality, Sprite itemIcon, float damage, float armor, float health)
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
                slot[i].GetComponent<Slot>().damage = damage;
                slot[i].GetComponent<Slot>().armor = armor;
                slot[i].GetComponent<Slot>().health = health;

                itemObj.transform.parent = slot[i].transform;
                itemObj.SetActive(false);

                slot[i].GetComponent<Slot>().UpdateSlot();
                slot[i].GetComponent<Slot>().empty = false;
                break;
            }
        }

        gameManager.SaveGame();
    }

    private void SetSlot(int i, GameObject itemObj)
    {
        Slot s = slot[i].GetComponent<Slot>();
        Item item = itemObj.GetComponent<Item>();
        s.icon = item.icon;
        s.type = item.type;
        s.description = item.description;
        s.ID = item.ID;
        s.item = itemObj;
        s.quality = item.quality;
        s.itemName = item.itemName;
        s.damage = item.damage;
        s.armor = item.armor;
        s.health = item.health;

        s.UpdateSlot();
        s.empty = false;
    }

    public void LoadByIds(List<int> ids)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            if (ids[i] != -1)
                SetSlot(i, ItemDatabase.instance.GetItemByID(ids[i]));

        }
    }

    public List<int> GetItemIds()
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < slot.Length; i++)
        {
            Slot s = slot[i].GetComponent<Slot>();
            if (s.item != null)
                ids.Add(s.ID);
            else
                ids.Add(-1);
        }
        return ids;
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
        slot[i].GetComponent<Slot>().damage = 0;
        slot[i].GetComponent<Slot>().armor = 0;
        slot[i].GetComponent<Slot>().health = 0;
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

    public void FindGrey()
    {
        Transform panel;
        for (int i = 0; i < slot.Length; i++)
        {
            panel = slot[i].transform.GetChild(0);
            if (panel.GetComponent<Image>().color == Color.grey)
            {
                panel.GetComponent<Image>().color = Color.white;
            }
        }
    }
}
