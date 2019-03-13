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
        }

        if (Input.GetButtonDown("UseHealthPotion"))
        {
            if (player.Health != player.base_maxhealth)
            {
                GameObject potion = inventory.FindItemByType("Health Potion");
                float heal = potion.GetComponent<HealthPotions>().healPercent;
                if (potion != null)
                {
                    inventory.RemoveItem(potion);
                    player.AddHealth(heal);
                }
            }
            else
                print("Already at full health");
        }
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
        if (other.CompareTag("InterObject"))
        {
            currentInterObj = other.gameObject;
            currentInterObjScript = currentInterObj.GetComponent<InteractionObject>();
            if (currentInterObjScript.inventory)
            {
                inventory.AddItem(currentInterObj);
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
