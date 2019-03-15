﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    //Class for interactions with objects

    public GameObject currentInterObj = null;
    public InteractionObject currentInterObjScript = null;
    public Inventory hubChest;
    public Inventory equipment;

    private PlayerLevel playerLevel;
    private InteractableGUI interactableGUI;
    private Player player;
    private HealthPotions hpPot;
    private int priceToLevelUp;

    void Update()
    {
        if (Input.GetButtonDown("Interact") && currentInterObj)
        {
            if (currentInterObjScript.talks)
            {
                currentInterObjScript.Talk();

                if(currentInterObj.name == "LevelUpNPC")
                {
                    if (player.essence >= priceToLevelUp)
                    {                
                        if(player.essence > 0)
                        player.essence -= priceToLevelUp;
                        playerLevel.currentLevel++;
                        priceToLevelUp++;
                        player.SetEssenceText();
                        playerLevel.SetLevelText();
                        player.SetHealthByLevel();
                        player.SetHealthText();
                        interactableGUI.Hide();
                        interactableGUI.Show("Level up for: " + priceToLevelUp.ToString(), transform, new Vector2(0, 2f));
                    }
                }
            }
            if(currentInterObjScript.openable)
            {
                if (currentInterObj.name == "HubChest")
                {
                    hubChest.inventoryEnabled = !hubChest.inventoryEnabled;

                    if (hubChest.inventoryEnabled)
                    {
                        hubChest.inventoryUI.SetActive(true);
                    }
                    else
                        hubChest.inventoryUI.SetActive(false);
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
                    int id = potion.GetComponent<Item>().ID;
                    if (potion != null)
                    {
                        equipment.RemoveItem(id);
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
        hpPot = GetComponent<HealthPotions>();
        interactableGUI = FindObjectOfType<InteractableGUI>();
        playerLevel = GetComponent<PlayerLevel>();
        priceToLevelUp = 0;
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
            if (currentInterObjScript.collectable)
            {
                bool Equipped = false;
                if (item.equipable)
                {
                    Equipped = equipment.Equip(currentInterObj, item.ID, item.type, item.description, item.icon);
                }
                if (Equipped == false)
                hubChest.AddItem(currentInterObj, item.ID, item.type, item.description, item.icon);
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
            }
        }
    }
}

