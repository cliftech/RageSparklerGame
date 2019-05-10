using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InGameMenu : MonoBehaviour
{
    public GameObject Menu;
    public GameObject audioPanel;
    public GameObject buttonToSelectOnOpen;
    private EventSystem eventSystem;
    private bool isAudioPanelShowing;
    // Start is called before the first frame update
    void Awake()
    {
        eventSystem = FindObjectOfType<EventSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Menu"))
        {
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
