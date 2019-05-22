using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    public GameObject menu;
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
            if (!menu.activeSelf)
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
            isShowing = !menu.activeSelf;

            if (!isAudioPanelShowing)
            {
                if (menu.activeSelf)
                {
                    menu.SetActive(false);
                }
                else
                {
                    menu.SetActive(true);
                    eventSystem.SetSelectedGameObject(buttonToSelectOnOpen);
                    isAudioPanelShowing = false;
                }
            }
            else
            {
                menu.SetActive(true);
                audioPanel.SetActive(false);
                eventSystem.SetSelectedGameObject(buttonToSelectOnOpen);
            }
        }
    }

    public void ShowAudioPanel(bool b)
    {
        isAudioPanelShowing = b;
    }
    public void HideMainMenu()
    {
        StartCoroutine(DisableAfterOneFrame());
    }
    IEnumerator DisableAfterOneFrame()
    {
        yield return new WaitForEndOfFrame();
        eventSystem.SetSelectedGameObject(previouslySelected);
        player.playerMovement.SetEnabled(previousPlayerEnabled);
        menu.SetActive(false);
        isShowing = false;
    }
}
