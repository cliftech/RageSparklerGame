using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseClickCatcher : MonoBehaviour
{
    private EventSystem es;
    private GameObject selectedObj;

    private void Start()
    {
        es = FindObjectOfType<EventSystem>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            es.SetSelectedGameObject(selectedObj);
        }
        else
            selectedObj = es.currentSelectedGameObject;

    }
}
