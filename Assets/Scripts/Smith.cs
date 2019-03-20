using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Smith : MonoBehaviour
{
    private InteractableGUI interactableGUI;
    private Player player;
    private LevelManager levelManager;
    private string playerTag = "Player";
    private Action interactAction;
    

    public string interactTextToDisplay = "Upgrade weapon for";
    private int upgradePrice = 0;

    private void Awake()
    {
        interactableGUI = FindObjectOfType<InteractableGUI>();
        player = FindObjectOfType<Player>();
        levelManager = FindObjectOfType<LevelManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        interactAction = () => Interact();
        playerExitedBounds();
    }


    private void Interact()
    {
        Debug.Log("essence: " + player.essence.ToString() + " price: "+ upgradePrice.ToString() + " dmg: "+ player.attack1Dam.ToString());
        if (player.essence >= upgradePrice)
        {
            player.attack1Dam += 1f;
            player.attack2Dam += 1.5f;
            player.attack3Dam += 2f;
            player.downwardAttackDam += 1.5f;
            player.essence -= upgradePrice;
            upgradePrice = (upgradePrice + 5) * 2;
            player.statusGUI.UpdateEssenceText();
            interactableGUI.Hide();
            interactableGUI.Show(interactTextToDisplay + " " + upgradePrice.ToString(), transform, new Vector2(0, 2f));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerEnteredBounds();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerExitedBounds();
    }

    private void playerEnteredBounds()
    {
        interactableGUI.Show(interactTextToDisplay + " " + upgradePrice.ToString(), transform, new Vector2(0, 2f));
        player.SetInteractAction(interactAction);
    }
    private void playerExitedBounds()
    {
        interactableGUI.Hide();
        player.ClearInteractAction(interactAction);
    }
}
