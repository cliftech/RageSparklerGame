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
                    hubChest.inventoryEnabled = !hubChest.inventoryEnabled;

                    if (hubChest.inventoryEnabled)
                    {
                        EventSystem.current.SetSelectedGameObject(hubChest.slot[0]);
                        hubChest.inventoryUI.SetActive(true);
                        player.playerMovement.SetEnabled(false);
                        followCamera.SetEnabled(false);
                    }
                }
            }
        }
        if (Input.GetButtonDown("Close") && hubChest.inventoryEnabled)
        {
            hubChest.inventoryUI.SetActive(false);
            player.playerMovement.SetEnabled(true);
            followCamera.SetEnabled(true);
            EventSystem.current.SetSelectedGameObject(equipment.slot[0]);
            hubChest.HideToolTip(hubChest.slot[0]);
            hubChest.FindGrey();
            hubChest.inventoryEnabled = false;
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
    public void swap(GameObject itemObj, int itemID, string itemType, string itemDescription, string itemQuality, string itemName, Sprite itemIcon, float damage, float armor, float health)
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
        }
        else
        {
            hubChest.RemoveItem(itemID);
            equipment.Equip(itemObj, itemID, itemType, itemDescription, itemName, itemIcon, itemQuality, damage, armor, health);
        }           
    }

    public void CompareToolTips(GameObject slotas, Text visualText, Text textBox, GameObject toolTip)
    {
        Slot tmpslot = slotas.GetComponent<Slot>();
        Slot tmp2 = equipment.FindItemByType(tmpslot.type).GetComponent<Slot>();
        Transform panel = tmp2.transform.GetChild(0);
        tmp2.selected = true;
        panel.GetComponent<Image>().color = Color.grey;

        if (!tmp2.empty)
        {
            visualText.text = tmp2.GetToolTip();
            textBox.text = visualText.text;

            toolTip.SetActive(true);

            float xPos = tmp2.transform.position.x - equipment.slotPaddingHorizontal - 15;
            float yPos = tmp2.transform.position.y - tmp2.GetComponent<RectTransform>().sizeDelta.y - equipment.slotPaddingVertical;

            toolTip.transform.position = new Vector2(xPos, yPos);
        }
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
                currentInterObj = null;
                hubChest.inventoryEnabled = false;
                hubChest.inventoryUI.SetActive(false);
                interactableGUI.Hide();
                equipment.toolTipObject.SetActive(false);
                hubChest.toolTipObject.SetActive(false);
            }
            player.playerMovement.SetEnabled(true);
            followCamera.SetEnabled(true);
        }
    }
}

