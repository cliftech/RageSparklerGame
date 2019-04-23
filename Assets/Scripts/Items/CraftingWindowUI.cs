using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingWindowUI : MonoBehaviour
{
    private bool craftEnabled;
    private Player player;
    private CameraController followCamera;
    private CraftingWindow craftWindow;
    private PlayerInteract plrInter;


    private void Awake()
    {
        player = FindObjectOfType<Player>();
        followCamera = FindObjectOfType<CameraController>();
        craftWindow = player.craftWindow;
        plrInter = player.plrInter;
    }

    void Update()
    {
        if (plrInter.currentInterObj && plrInter.currentInterObj.name == "CraftingNPC" && Input.GetButtonDown("Interact") && !craftEnabled)
        {
            Slot current = null;
            craftWindow.gameObject.SetActive(true);
            craftEnabled = true;
            for (int i = 0; i < craftWindow.craftingRecipeUIs.Count; i++)
            {
                craftWindow.craftingRecipeUIs[i].UpdateAllCraftingRecipes();
                if (craftWindow.craftingRecipeUIs[i].gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(craftWindow.craftingRecipeUIs[i].slots[0].gameObject);
                    craftWindow.equipment.ShowToolTip(craftWindow.craftingRecipeUIs[i].slots[0]);
                    current = craftWindow.craftingRecipeUIs[i].slots[0];
                    break;
                }
            }
            player.playerMovement.SetEnabled(false);
            followCamera.SetEnabled(false);
            craftWindow.equipment.toolTipObject.SetActive(false);
            craftWindow.equipment.compareToolTipObject.SetActive(false);
            craftWindow.hubChest.toolTipObject.SetActive(false);
            craftWindow.hubChest.compareToolTipObject.SetActive(false);
            craftWindow.equipment.invsOpen++;
            if(current != null)
                craftWindow.equipment.ShowToolTip(current);
        }
        else if (plrInter.currentInterObj && plrInter.currentInterObj.name == "CraftingNPC" && Input.GetButtonDown("Interact") && craftEnabled)
        {
            craftWindow.gameObject.SetActive(false);
            craftEnabled = false;
            craftWindow.equipment.invsOpen--;
            if (craftWindow.equipment.invsOpen == 0)
            {
                player.playerMovement.SetEnabled(true);
                followCamera.SetEnabled(true);
            }
            craftWindow.FindGrey();
            craftWindow.equipment.toolTipObject.SetActive(false);
            craftWindow.equipment.compareToolTipObject.SetActive(false);
            craftWindow.hubChest.toolTipObject.SetActive(false);
            craftWindow.hubChest.compareToolTipObject.SetActive(false);

            if(craftWindow.equipment.inventoryEnabled)
            {
                craftWindow.equipment.ShowToolTip(craftWindow.equipment.slots[0]);
            }
        }
    }
}
