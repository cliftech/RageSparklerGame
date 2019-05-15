using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    public GameObject Menu;
    public GameObject audioPanel;
    public GameObject buttonToSelectOnOpen;
    private EventSystem eventSystem;
    private bool isAudioPanelShowing;
    private bool isShowing;

    private static InGameMenu menuInstance;
    private GameObject previouslySelected;
    private bool previousPlayerEnabled;

    private Player player;

    public static bool isMenuShowing()
    {
        return menuInstance.isShowing;
    }

    // Start is called before the first frame update
    void Awake()
    {
        menuInstance = this;
        eventSystem = FindObjectOfType<EventSystem>();
        player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Menu"))
        {
            if (!Menu.activeSelf)
            {
                previouslySelected = eventSystem.currentSelectedGameObject;
                previousPlayerEnabled = !player.playerMovement.isDisabled;
                player.playerMovement.SetEnabled(false);
            }
            else
            {
                eventSystem.SetSelectedGameObject(previouslySelected);
                player.playerMovement.SetEnabled(previousPlayerEnabled);
            }
            isShowing = !Menu.activeSelf;

            if (!isAudioPanelShowing)
            {
                if (Menu.activeSelf)
                {
                    Menu.SetActive(false);
                }
                else
                {
                    Menu.SetActive(true);
                    eventSystem.SetSelectedGameObject(buttonToSelectOnOpen);
                    isAudioPanelShowing = false;
                }
            }
            else
            {
                Menu.SetActive(true);
                audioPanel.SetActive(false);
                eventSystem.SetSelectedGameObject(buttonToSelectOnOpen);
            }
        }
    }

    public void ShowAudioPanel(bool b)
    {
        isAudioPanelShowing = b;
    }
}
