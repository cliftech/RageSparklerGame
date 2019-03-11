using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Portal : MonoBehaviour
{
    private InteractableGUI interactableGUI;
    private Player player;
    private LevelManager levelManager;
    private string playerTag = "Player";
    private Action interactAction;

    public GameObject levelToLoad;
    public string interactTextToDisplay = "to blah";

    public bool forceEntry = false;
    public bool actsAsCheckpoint = true;
    private bool checkpointActivated = false;


    void Awake()
    {
        interactableGUI = FindObjectOfType<InteractableGUI>();
        player = FindObjectOfType<Player>();
        levelManager = FindObjectOfType<LevelManager>();
    }
    void Start()
    {
        interactAction = () => ActivatePortal();
        playerExitedBounds();
    }

    private void playerEnteredBounds()
    {
        interactableGUI.Show(interactTextToDisplay, transform, new Vector2(0, 2f));
        player.SetInteractAction(interactAction);
    }
    private void playerExitedBounds()
    {
        interactableGUI.Hide();
        player.ClearInteractAction(interactAction);
    }

    private void ActivatePortal()
    {
        if(levelToLoad != null)
            levelManager.LoadLevel(levelToLoad);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (actsAsCheckpoint && !checkpointActivated)
            {
                player.SetRespawnPos(transform.position);
                checkpointActivated = true;
            }
            if (!forceEntry)
                playerEnteredBounds();
            else
                ActivatePortal();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerExitedBounds();
    }
}
