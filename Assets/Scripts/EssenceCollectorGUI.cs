using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssenceCollectorGUI : MonoBehaviour
{
    public RectTransform canvasRect;

    private bool showing;
    private Vector2 hiddenPos;
    private Transform target;
    private Vector2 offset;
    private Action deposit;
    private Action withdraw;

    void Start()
    {
        hiddenPos = new Vector2(-10000, -10000);
        Hide();
    }
    private void Update()
    {
        if (showing)
        {
            if (Input.GetButtonDown("Interact"))
            {
                deposit.Invoke();
            }
            else if (Input.GetButtonDown("Withdraw"))
            {
                withdraw.Invoke();
            }
        }
    }
    void LateUpdate()
    {
        if (showing)
        {
            // Final position of marker above GO in world space
            Vector3 offsetPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, target.transform.position.z);

            // Calculate *screen* position (note, not a canvas/recttransform position)
            Vector2 canvasPos;
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

            // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out canvasPos);

            // Set
            transform.localPosition = canvasPos;
        }
    }
    public void Show(Transform target, Vector2 offset, Action deposit, Action withdraw)
    {
        this.target = target;
        this.offset = offset;
        this.deposit = deposit;
        this.withdraw = withdraw;
        showing = true;
    }
    public void Hide()
    {
        transform.position = hiddenPos;
        showing = false;
    }
}
