using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    public GameObject currentInterObj = null;
    public InteractionObject currentInterObjScript = null;
    public Inventory hubChest;
    public Inventory equipment;

    private InteractableGUI interactableGUI;
    private Player player;
    private CameraController followCamera;
    private Canvas canvas;

    void Update()
    {
        if (Input.GetButtonDown("Interact") && currentInterObj)
        {
            if(currentInterObjScript.talks)
            {                
            }
            if(currentInterObjScript.openable)
            {
                hubChest.toolTipObject.SetActive(false);
                equipment.toolTipObject.SetActive(false);
                if (currentInterObj.name == "HubChest" && !hubChest.inventoryEnabled)
                {
                    equipment.invsOpen++;
                    hubChest.inventoryEnabled = !hubChest.inventoryEnabled;
                    EventSystem.current.SetSelectedGameObject(hubChest.slots[0].gameObject);
                    hubChest.inventoryUI.SetActive(true);
                    player.playerMovement.SetEnabled(false);
                    followCamera.SetEnabled(false);
                    hubChest.ShowToolTip(hubChest.slots[0]); 
                    hubChest.CompareToolTips(hubChest.slots[0]);
                }
                else if (currentInterObj.name == "HubChest" && hubChest.inventoryEnabled)
                {
                    hubChest.inventoryUI.SetActive(false);
                    equipment.invsOpen--;
                    if (equipment.invsOpen == 0)
                    {
                        player.playerMovement.SetEnabled(true);
                        followCamera.SetEnabled(true);
                    }
                    EventSystem.current.SetSelectedGameObject(equipment.slots[0].gameObject);              
                    hubChest.HideToolTip(hubChest.slots[0]); 
                    hubChest.HideCompareToolTips();
                    hubChest.FindGrey();
                    hubChest.inventoryEnabled = false;
                    if(equipment.inventoryEnabled)
                    equipment.ShowToolTip(equipment.slots[0]); 
                }
            }
            
        }

        if (Input.GetButtonDown("UseHealthPotion"))
        {
            if (player.Health != player.base_maxhealth)
            {
                Item potion = equipment.GetPotion();
                if (potion != null)
                {
                    float heal = potion.healPercent;
                    equipment.RemoveItem(potion);
                    player.AddHealth(heal);
                    
                }
            }
        }
        if(Input.GetButtonDown("SortInventory") && hubChest.inventoryEnabled)
        {
            Sort();
        }
    }

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        hubChest = GetComponent<Inventory>();
        player = GetComponent<Player>();
        followCamera = GameObject.Find("Main Camera").GetComponent<CameraController>();
        interactableGUI = Resources.FindObjectsOfTypeAll<InteractableGUI>()[0];
    }

    //swap player inventory item with hub chest inventory item
    public void swap(Item item, int whichSlot)
    {
        int chestSelectedSlotIndex = hubChest.FindGrayIndex();
        int equipSelectedSlotIndex = equipment.FindGrayIndex();

        if (hubChest.RemoveItem(item))
        {
            Item previousItem;
            if (equipment.Equip(item, out previousItem) && previousItem != null)
            {
                hubChest.AddItemToHubChest(previousItem);
                hubChest.ShowToolTip(hubChest.slots[whichSlot]);
                hubChest.CompareToolTips(hubChest.slots[whichSlot]);
            }
            else
            {
                hubChest.HideToolTip(hubChest.slots[whichSlot]);
                hubChest.HideCompareToolTips();
            }
        }
        else
        {
            hubChest.AddItemToHubChest(item);
        }

        if (chestSelectedSlotIndex != -1)
            hubChest.slots[chestSelectedSlotIndex].slotIcon.GetComponent<Image>().color = Color.gray;

        if (equipSelectedSlotIndex != -1)
            equipment.slots[equipSelectedSlotIndex].slotIcon.GetComponent<Image>().color = Color.gray;
    }

    public void Unequip(Item item, Slot slot)
    {
        int equipSelectedSlotIndex = equipment.FindGrayIndex();

        Item tempItem = null;
        tempItem = equipment.RemoveItemByID(item.ID);
        if(tempItem != null)
        {
            player.SetItemStats();
            equipment.HideToolTip(slot);
            hubChest.AddItemToHubChest(tempItem);
        }

        if (equipSelectedSlotIndex != -1)
            equipment.slots[equipSelectedSlotIndex].slotIcon.GetComponent<Image>().color = Color.gray;
    }

    public void Sort()
    {
        int selectedSlotIndex = hubChest.FindGrayIndex();
        hubChest.slots = (from x in hubChest.slots
                          where x.itemas != null
                          select x).OrderBy(x => x.itemas.type).ThenBy(x => x.itemas.name).ToArray();
        Refresh();
        if (selectedSlotIndex != -1)
            hubChest.slots[selectedSlotIndex].slotIcon.GetComponent<Image>().color = Color.gray;

    }
    public void Refresh()
    {
        List<Item> templist = new List<Item>(hubChest.slots.Length);
        Item tempItem = null;
        int amount;
        for (int i = 0; i < hubChest.slots.Length; i++)
        {
            if (hubChest.slots[i].itemas.type == ItemType.Material)
            {
                amount = hubChest.slots[i].Amount;
                for (int j = 0; j < amount; j++)
                {
                    tempItem = hubChest.RemoveItemByID(hubChest.slots[i].itemas.ID);
                    templist.Add(tempItem);
                }
            }
            else
            {
                tempItem = hubChest.RemoveItemByID(hubChest.slots[i].itemas.ID);
                templist.Add(tempItem);
            }
        }
        hubChest.slots = hubChest.itemsParent.GetComponentsInChildren<Slot>();
        hubChest.RemoveAll();
        for (int i = 0; i < templist.Count; i++)
        {
            hubChest.AddItemToHubChest(templist[i]);
        }
    }

    public void CompareToolTips(Slot slot, Text visualText, Text textBox, GameObject cmpToolTip, GameObject toolTip)
    {
        Slot tmpslot = slot;
        Slot tmp2;
        if (tmpslot.itemas != null && tmpslot.itemas.type.ToString() != "Material")
        {
            tmp2 = equipment.FindItemByType(tmpslot.itemas.type.ToString());
            if (tmp2.itemas != null)
            {
                tmp2.CompareItems(tmpslot);
                visualText.text = tmp2.GetToolTip(true);
                textBox.text = visualText.text;

                Canvas.ForceUpdateCanvases();
                float offset = toolTip.transform.Find("TextBox").GetComponent<RectTransform>().rect.height - toolTip.transform.Find("TextBox").Find("BackGround").GetComponent<RectTransform>().offsetMin.y;
                float xPos = slot.transform.position.x - hubChest.slotPaddingHorizontal - 35;
                float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - hubChest.slotPaddingVertical - 15;

                cmpToolTip.transform.position = new Vector2(xPos, yPos);
                cmpToolTip.transform.GetComponent<RectTransform>().localPosition = new Vector3(toolTip.transform.GetComponent<RectTransform>().localPosition.x, toolTip.transform.GetComponent<RectTransform>().localPosition.y - offset);
                cmpToolTip.SetActive(true);
                cmpToolTip.transform.GetComponent<HorizontalLayoutGroup>().enabled = false;
                cmpToolTip.transform.GetComponent<HorizontalLayoutGroup>().enabled = true;
                Canvas.ForceUpdateCanvases();
                ClampToWindow(cmpToolTip);
            }
        }
    }
    public void HideCompareToolTips(GameObject toolTip)
    {
        toolTip.SetActive(false);
    }

    public void ClampToWindow(GameObject toolTip)
    {
        Vector3 pos = toolTip.transform.GetComponent<RectTransform>().localPosition;

        Vector3 minPosition = canvas.transform.GetComponent<RectTransform>().rect.min - toolTip.transform.GetComponent<RectTransform>().rect.min;
        Vector3 maxPosition = canvas.transform.GetComponent<RectTransform>().rect.max - toolTip.transform.GetComponent<RectTransform>().rect.max;

        pos.x = Mathf.Clamp(toolTip.transform.GetComponent<RectTransform>().localPosition.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(toolTip.transform.GetComponent<RectTransform>().localPosition.y, minPosition.y, maxPosition.y);

        toolTip.transform.GetComponent<RectTransform>().localPosition = pos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("InterObject") || other.CompareTag("Item"))
        {
            currentInterObj = other.gameObject;

            if (currentInterObj.name == "CraftingNPC")
                interactableGUI.Show("Craft", currentInterObj.transform, new Vector2(0, 2f));

            currentInterObjScript = currentInterObj.GetComponent<InteractionObject>();
            if (currentInterObjScript.openable)
                interactableGUI.Show("Open", currentInterObj.transform, new Vector2(0, 2f));

            if (currentInterObjScript.collectable)
            {
                Item item = currentInterObj.GetComponent<PickUp>().item;
                Item previousItem;
                if (equipment.Equip(item, out previousItem) && previousItem != null)
                {
                    hubChest.AddItemToHubChest(previousItem);
                }
                else if(item.type.ToString() == "Material")
                {
                    hubChest.AddItemToHubChest(item);
                }
                currentInterObj.SetActive(false);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("InterObject"))
        {
            if (other.gameObject == currentInterObj)
            {
                if(other.gameObject.name == "HubChest")
                {
                    if(hubChest.inventoryEnabled)
                        equipment.invsOpen--;
                    if (equipment.invsOpen == 0)
                    {
                        player.playerMovement.SetEnabled(true);
                        followCamera.SetEnabled(true);
                    }
                }
                currentInterObj = null;
                hubChest.inventoryEnabled = false;
                hubChest.inventoryUI.SetActive(false);
                interactableGUI.Hide();
                equipment.toolTipObject.SetActive(false);
                hubChest.toolTipObject.SetActive(false);
                hubChest.HideCompareToolTips();
                if (equipment.inventoryEnabled)
                {
                    EventSystem.current.SetSelectedGameObject(equipment.slots[0].gameObject);
                    equipment.ShowToolTip(equipment.slots[0]);
                }
            }           
        }
    }
}

