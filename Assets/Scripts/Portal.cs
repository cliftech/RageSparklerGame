using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Portal : MonoBehaviour
{
    private PortalGUI portalGUI;
    private InteractableGUI interactableGUI;
    private Player player;
    private LevelManager levelManager;
    private string playerTag = "Player";
    private Action interactAction;

    public GameObject levelToLoad;
    public string interactTextToDisplay = "to blah";
    //Portal id is  used to check if player unlocked this point and can get to it from hub. Id "XY" means that portal
    //is in X area and is the Yth portal. Y index starts from 0.
    public int portalId;

    public bool forceEntry = false;
    public bool isInHub = false;
    public bool isCheckpint = false;
    private bool checkpointActivated = false;
    private Vector2 guiOffset;


    void Awake()
    {
        portalGUI = FindObjectOfType<PortalGUI>();
        interactableGUI = FindObjectOfType<InteractableGUI>();
        player = FindObjectOfType<Player>();
        levelManager = FindObjectOfType<LevelManager>();
    }
    void Start()
    {
        guiOffset = new Vector2(0, 2f);
        interactAction = () => ActivatePortal();
        playerExitedBounds();
    }

    private void playerEnteredBounds()
    {
        Level level = levelToLoad.GetComponent<Level>();
        portalGUI.SetOption(0, () => ActivateHubPortal(-1), level.title + " - Start");
        if (isInHub && level.checkPoints.Count > 0)
        {
            int index = 1;
            foreach(var Checkpoint in level.checkPoints)
            {
                if(player.Checkpoints.Contains(Checkpoint.portalId))
                {
                    portalGUI.SetOption(index, () => ActivateHubPortal(Checkpoint.portalId), level.title + " - " + index);
                    index++;
                }
            }
            //for (int i = 0; i < level.checkPoints.Count; i++)
            //{
            //    int index = i;
            //    portalGUI.SetOption(i+1, () => ActivateHubPortal(index), level.title + " - " + index);
            //}
            interactableGUI.Show(interactTextToDisplay, transform, guiOffset + Vector2.up * 2.1f);
            portalGUI.Show(transform, guiOffset);
        }
        else
        {
            interactableGUI.Show(interactTextToDisplay, transform, guiOffset);
            player.SetInteractAction(interactAction);
        }
    }

    private void playerExitedBounds()
    {
        interactableGUI.Hide();
        player.ClearInteractAction(interactAction);
        portalGUI.Hide();
    }

    private void ActivatePortal()
    {
        if(levelToLoad != null)
            levelManager.LoadLevel(levelToLoad);
    }
    private void ActivateHubPortal(int indexSelected)
    {

        if (levelToLoad != null)
            levelManager.LoadLevel(levelToLoad, indexSelected);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!checkpointActivated)
            {
                player.SetRespawnPos(transform.position);
                checkpointActivated = true;
            }
            if(isCheckpint)
            {
                player.Checkpoints.Add(portalId);
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
