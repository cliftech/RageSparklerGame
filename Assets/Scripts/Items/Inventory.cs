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
    public Transform itemsParent;

    public Slot[] slots;
    public List<Item> startingItems;

    public int totalSlots;
    public int invsOpen;

    public float slotPaddingHorizontal;
    public float slotPaddingVertical;

    private void OnValidate()
    {
        if (itemsParent != null)
        {
            slots = itemsParent.GetComponentsInChildren<Slot>();
            SetStartingItems();
        }
    }

    void Awake()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    void Start()
    {
        if (itemsParent != null)
        {
            slots = itemsParent.GetComponentsInChildren<Slot>();
            SetStartingItems();
        }
        visualText = visualTextObject;
        textBox = textBoxObject;
        toolTip = toolTipObject;
        compareToolTip = compareToolTipObject;
        compareTextBox = compareTextBoxObject;
        compareVisualText = compareVisualTextObject;
        plrInter = plrInterObject;
        player = GetComponent<Player>();
        followCamera = GameObject.Find("Main Camera").GetComponent<CameraController>();
        invsOpen = 0;

        for (int i = 0; i < totalSlots; i++)
        {
            slots[i].whichSlot = i;
        }
    }


    private void SetStartingItems()
    {
        int i = 0;
        for (; i < startingItems.Count && i < slots.Length; i++)
        {
            slots[i].itemas = startingItems[i];
            slots[i].Amount = 1;
        }

        for (; i < slots.Length; i++)
        {
            slots[i].itemas = null;
            slots[i].Amount = 0;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("OpenInv") && inventoryUI.name == "EquipmentUI" && !inventoryEnabled)
        {
            inventoryEnabled = true;
            EventSystem.current.SetSelectedGameObject(slots[0].gameObject);
            inventoryUI.SetActive(true);
            player.playerMovement.SetEnabled(false);
            followCamera.SetEnabled(false);
            toolTipObject.SetActive(false);
            compareToolTipObject.SetActive(false);
            ShowToolTip(slots[0]);
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
            EventSystem.current.SetSelectedGameObject(plrInter.hubChest.slots[0].gameObject);
            inventoryEnabled = false;
            FindGrey();
            toolTipObject.SetActive(false);
            compareToolTipObject.SetActive(false);
            if (plrInter.hubChest.inventoryEnabled)
            {
                ShowToolTip(plrInter.hubChest.slots[0]);
                CompareToolTips(plrInter.hubChest.slots[0]);
            }
        }
    }

    public void ShowToolTip(Slot slot)
    {
        Slot tmpslot = slot;
        Slot equipped;
        Transform panel = tmpslot.transform.GetChild(0);
        //tmpslot.selected = true;
        panel.GetComponent<Image>().color = Color.grey;

        if (tmpslot.itemas != null)
        {
            if(tmpslot.itemas.type.ToString() != "Material")
            {
                equipped = plrInter.equipment.FindItemByType(tmpslot.itemas.type.ToString());
                if (equipped.itemas != null)
                    tmpslot.CompareItems(equipped);
            }
            visualText.text = tmpslot.GetToolTip(false);
            textBox.text = visualText.text;

            toolTip.SetActive(true);
            Canvas.ForceUpdateCanvases();
            float xPos = slot.transform.position.x - slotPaddingHorizontal - 35;
            float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - slotPaddingVertical - 15;
            toolTip.transform.position = new Vector2(xPos, yPos);
            toolTip.transform.GetComponent<HorizontalLayoutGroup>().enabled = false;
            toolTip.transform.GetComponent<HorizontalLayoutGroup>().enabled = true;
            Canvas.ForceUpdateCanvases();
            plrInter.ClampToWindow(toolTip);
        }
    }

    public void CompareToolTips(Slot slot)
    {
        plrInter.CompareToolTips(slot, compareVisualText, compareTextBox, compareToolTip, toolTip);
    }

    public void HideToolTip(Slot slot)
    {
        Slot tmpslot = slot;
        Transform panel = tmpslot.transform.GetChild(0);
        panel.GetComponent<Image>().color = Color.white;
        toolTip.SetActive(false);
        //tmpslot.selected = false;
    }
    public void HideCompareToolTips()
    {
        plrInter.HideCompareToolTips(compareToolTip);
    }

    public bool Equip(Item item, out Item previousItem)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].equipType == item.type)
            {
                previousItem = slots[i].itemas;
                slots[i].itemas = item;
                slots[i].Amount = 1;
                player.SetItemStats();
                gameManager.SaveGame();
                return true;
            }
        }
        previousItem = null;
        return false;
    }

    public bool AddItemToHubChest(Item item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemas == null || slots[i].itemas.ID == item.ID && item.MaximumStack > slots[i].Amount)
            {
                if (item.type.ToString() != "Material")
                {
                    slots[i].itemas = Instantiate(item);
                    slots[i].Amount++;
                }
                else
                {
                    slots[i].itemas = item;
                    slots[i].Amount++;
                }
                gameManager.SaveGame();
                return true;
            }
        }
        return false;
    }

    public int ItemCount(Item item)
    {
        int amount = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemas != null)
            {
                if (slots[i].itemas.itemName == item.itemName)
                    amount += slots[i].Amount;
            }
        }
        return amount;
    }

    public void LoadByIds(List<string> ids, List<int> amounts)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            if (ids[i].ToString() != "-1")
            {
                slots[i].itemas = ItemDatabase.instance.GetItemByID(ids[i]);
                slots[i].Amount = amounts[i];
            }
        }
    }

    public List<string> GetItemIds(out List<int> amounts)
    {
        List<string> ids = new List<string>();
        amounts = new List<int>();
        for (int i = 0; i < slots.Length; i++)
        {
            Slot s = slots[i];
            if (s.itemas != null)
            {
                ids.Add(s.itemas.ID);
                amounts.Add(s.Amount);
            }
            else
            {
                ids.Add("-1");
                amounts.Add(-1);
            }
        }
        return ids;
    }

    public Item GetPotion()
    {
        return slots[8].itemas;
    }

    public Item RemoveItemByID(string itemID)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Item item = slots[i].itemas;
            if (item != null && item.ID == itemID)
            {
                slots[i].Amount--;
                if (slots[i].Amount == 0)
                {
                    slots[i].itemas = null;
                }
                return item;
            }
        }
        return null;
    }

    public void RemoveAll()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].itemas = null;
            slots[i].Amount = 0;
        }
    }

    public bool RemoveItem(Item item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemas == item)
            {
                slots[i].Amount--;
                if (slots[i].Amount == 0)
                {
                    slots[i].itemas = null;
                }
                return true;
            }
        }
        return false;
    }

    public Slot FindItemByType(string itemType)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                if (slots[i].name == itemType)
                {
                    return slots[i];
                }
            }
        }
        return null;
    }

    public void FindGrey()
    {
        Transform panel;
        for (int i = 0; i < slots.Length; i++)
        {
            panel = slots[i].transform.GetChild(0);
            if (panel.GetComponent<Image>().color == Color.grey)
            {
                panel.GetComponent<Image>().color = Color.white;
            }
        }
    }
}
