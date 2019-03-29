using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    //Class for interactions with objects

    public GameObject currentInterObj = null;
    public InteractionObject currentInterObjScript = null;
    public Inventory hubChest;
    public Inventory equipment;

    private InteractableGUI interactableGUI;
    private Player player;
    private CameraController followCamera;
    private HealthPotions hpPot;
    private int priceToLevelUp;

    void Update()
    {
        if (Input.GetButtonDown("Interact") && currentInterObj)
        {
            if (currentInterObjScript.talks)
            {
                if(currentInterObj.name == "LevelUpNPC")
                {
                    if (player.essence >= priceToLevelUp)
                    {                
                        if(player.essence > 0)
                        player.essence -= priceToLevelUp;
                        player.LevelUp();
                        priceToLevelUp++;
                        player.statusGUI.UpdateEssenceText();
                        player.SetHealthByLevel();
                        player.statusGUI.UpdateHealthbar();
                        interactableGUI.Hide();
                        interactableGUI.Show("Level up for: " + priceToLevelUp.ToString(), transform, new Vector2(0, 2f));
                    }
                }
            }
            if(currentInterObjScript.openable)
            {
                hubChest.toolTipObject.SetActive(false);
                equipment.toolTipObject.SetActive(false);
                if (currentInterObj.name == "HubChest" && !hubChest.inventoryEnabled)
                {
                    equipment.invsOpen++;
                    hubChest.inventoryEnabled = !hubChest.inventoryEnabled;
                    EventSystem.current.SetSelectedGameObject(hubChest.slot[0]);
                    hubChest.inventoryUI.SetActive(true);
                    player.playerMovement.SetEnabled(false);
                    followCamera.SetEnabled(false);
                    hubChest.ShowToolTip(hubChest.slot[0]);
                    hubChest.CompareToolTips(hubChest.slot[0]);
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
                    EventSystem.current.SetSelectedGameObject(equipment.slot[0]);                    
                    hubChest.HideToolTip(hubChest.slot[0]);
                    hubChest.HideCompareToolTips();
                    hubChest.FindGrey();
                    hubChest.inventoryEnabled = false;
                    if(equipment.inventoryEnabled)
                    equipment.ShowToolTip(equipment.slot[0]);
                }
            }
            
        }    

        if (Input.GetButtonDown("UseHealthPotion"))
        {
            if (player.Health != player.base_maxhealth)
            {
                GameObject potion = equipment.GetPotion();
                if (potion != null)
                {
                    float heal = potion.GetComponent<HealthPotions>().healPercent;
                    string type = potion.GetComponent<Item>().type;
                    if (potion != null)
                    {
                        equipment.RemoveItem(type);
                        player.AddHealth(heal);
                    }
                }
            }
        }
    }

    void Start()
    {
        hubChest = GetComponent<Inventory>();
        player = GetComponent<Player>();
        followCamera = GameObject.Find("Main Camera").GetComponent<CameraController>();
        hpPot = GetComponent<HealthPotions>();
        interactableGUI = FindObjectOfType<InteractableGUI>();
        priceToLevelUp = 0;
    }

    //swap player inventory item with hub chest inventory item
    public void swap(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemQuality, string itemName, Sprite itemIcon, float damage, float armor, float health, int whichSlot)
    {
        GameObject which = equipment.FindItemByType(itemType);
        if(!which.GetComponent<Slot>().empty)
        {
            hubChest.RemoveItem(itemID);
            hubChest.AddItemToHubChest(which.GetComponent<Slot>().item, which.GetComponent<Slot>().ID, which.GetComponent<Slot>().type, which.GetComponent<Slot>().description,
               which.GetComponent<Slot>().itemName, which.GetComponent<Slot>().quality, which.GetComponent<Slot>().icon, which.GetComponent<Slot>().damage,
               which.GetComponent<Slot>().armor, which.GetComponent<Slot>().health);

            equipment.RemoveItem(itemType);
            equipment.Equip(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, itemQuality, damage, armor, health);
            hubChest.ShowToolTip(hubChest.slot[whichSlot]);
            hubChest.CompareToolTips(hubChest.slot[whichSlot]);
        }
        else
        {
            hubChest.RemoveItem(itemID);
            equipment.Equip(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, itemQuality, damage, armor, health);
        }
    }

    public void CompareToolTips(GameObject slot, Text visualText, Text textBox, GameObject cmpToolTip, GameObject toolTip)
    {
        Slot tmpslot = slot.GetComponent<Slot>();
        Slot tmp2;
        if (!tmpslot.empty)
        {
            tmp2 = equipment.FindItemByType(tmpslot.type).GetComponent<Slot>();
            if (!tmp2.empty)
            {
                visualText.text = tmp2.GetToolTip(true);
                textBox.text = visualText.text;

                float offset = toolTip.transform.Find("TextBox").GetComponent<RectTransform>().rect.height - toolTip.transform.Find("TextBox").Find("BackGround").GetComponent<RectTransform>().offsetMin.y;
                float xPos = slot.transform.position.x - hubChest.slotPaddingHorizontal - 35;
                float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - hubChest.slotPaddingVertical - 15;

                cmpToolTip.transform.position = new Vector2(xPos, yPos);
                cmpToolTip.transform.GetComponent<RectTransform>().localPosition = new Vector3(toolTip.transform.GetComponent<RectTransform>().localPosition.x, toolTip.transform.GetComponent<RectTransform>().localPosition.y - offset - 2);
                cmpToolTip.SetActive(true);
            }
        }
    }
    public void HideCompareToolTips(GameObject toolTip)
    {
        toolTip.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("InterObject") || other.CompareTag("Item"))
        {
            currentInterObj = other.gameObject;
            if (currentInterObj.name == "LevelUpNPC")
                interactableGUI.Show("Level up for: " + priceToLevelUp.ToString(), transform, new Vector2(0, 2f));
            
            Item item = currentInterObj.GetComponent<Item>();
            currentInterObjScript = currentInterObj.GetComponent<InteractionObject>();
            if (currentInterObjScript.openable)
                interactableGUI.Show("Open", transform, new Vector2(0, 2f));
            if (currentInterObjScript.collectable)
            {
                bool Equipped = false;
                if (item.equipable)
                {
                    Equipped = equipment.Equip(currentInterObj, item.ID, item.type, item.description, item.itemName, item.icon, item.quality, item.damage, item.armor, item.health);
                }
                if (Equipped == false)
                hubChest.AddItemToHubChest(currentInterObj, item.ID, item.type, item.description, item.itemName, item.quality, item.icon, item.damage, item.armor, item.health);
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
                if (equipment.inventoryEnabled)
                {
                    EventSystem.current.SetSelectedGameObject(equipment.slot[0]);
                    equipment.ShowToolTip(equipment.slot[0]);
                }
            }           
        }
    }
}

