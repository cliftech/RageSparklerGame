using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    //Class for interactions with objects

    public GameObject currentInterObj = null;
    public InteractionObject currentInterObjScript = null;
    public Inventory inventory;

    private Player player;
    private HealthPotions hpPot;
    private PlayerLevel level;

    void Update()
    {
        if (Input.GetButtonDown("Interact") && currentInterObj)
        {
            if (currentInterObjScript.talks)
            {
                currentInterObjScript.Talk();

                if(currentInterObj.name == "LevelUpNPC")
                {
                    if (player.coins >= level.xpToLevelUp[level.currentLevel])
                    {                
                        if(player.coins > 0)
                        player.coins -= level.xpToLevelUp[level.currentLevel];
                        level.currentLevel++;
                        player.SetCoinText();
                        level.SetLevelText();
                        print("Level up!");
                        player.SetHealthByLevel();
                        player.SetHealthText();
                    }
                    else
                        print("You need " + level.xpToLevelUp[level.currentLevel] + " coins to level up!");
                }
            }
            if(currentInterObjScript.openable)
            {
                if (currentInterObj.name == "HubChest")
                {
                    inventory.inventoryEnabled = !inventory.inventoryEnabled;

                    if (inventory.inventoryEnabled)
                    {
                        inventory.inventoryUI.SetActive(true);
                    }
                    else
                        inventory.inventoryUI.SetActive(false);
                }
            }
        }

    //    if (Input.GetButtonDown("UseHealthPotion"))
    //    {
    //        if (player.Health != player.base_maxhealth)
    //        {
    //            GameObject potion = inventory.FindItemByType("Health Potion");
    //            float heal = potion.GetComponent<HealthPotions>().healPercent;
    //            if (potion != null)
    //            {
    //                inventory.RemoveItem(potion);
    //                player.AddHealth(heal);
    //            }
    //        }
    //        else
    //            print("Already at full health");
    //    }
    }
    void Start()
    {
        inventory = GetComponent<Inventory>();
        player = GetComponent<Player>();
        hpPot = GetComponent<HealthPotions>();
        level = GetComponent<PlayerLevel>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("InterObject") || other.CompareTag("Item"))
        {
            currentInterObj = other.gameObject;
            Item item = currentInterObj.GetComponent<Item>();
            currentInterObjScript = currentInterObj.GetComponent<InteractionObject>();
            if (currentInterObjScript.collectable)
            {
                inventory.AddItem(currentInterObj, item.ID, item.type, item.description, item.icon);
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
            }
        }
    }
}

