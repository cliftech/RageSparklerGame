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


    private void Awake()
    {
        player = FindObjectOfType<Player>();
        followCamera = FindObjectOfType<CameraController>();
        craftWindow = player.craftWindow;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !craftEnabled)
        {
            craftWindow.gameObject.SetActive(true);
            craftEnabled = true;
            EventSystem.current.SetSelectedGameObject(craftWindow.craftingRecipeUIs[0].slots[0].gameObject);
            player.playerMovement.SetEnabled(false);
            followCamera.SetEnabled(false);
            craftWindow.equipment.toolTipObject.SetActive(false);
            craftWindow.equipment.compareToolTipObject.SetActive(false);
            craftWindow.hubChest.toolTipObject.SetActive(false);
            craftWindow.hubChest.compareToolTipObject.SetActive(false);
            craftWindow.equipment.invsOpen++;
            craftWindow.equipment.ShowToolTip(craftWindow.craftingRecipeUIs[0].slots[0]);
        }
        else if (Input.GetKeyDown(KeyCode.C) && craftEnabled)
        {
            craftWindow.gameObject.SetActive(false);
            craftEnabled = false;
            craftWindow.equipment.invsOpen--;
            if (craftWindow.equipment.invsOpen == 0)
            {
                player.playerMovement.SetEnabled(true);
                followCamera.SetEnabled(true);
            }
            EventSystem.current.SetSelectedGameObject(craftWindow.craftingRecipeUIs[0].slots[0].gameObject);
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
