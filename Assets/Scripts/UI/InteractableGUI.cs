using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableGUI : MonoBehaviour
{
    public RectTransform canvasRect;
    public Text interactText;

    private bool showing;
    private Vector2 hiddenPos;
    private Transform target;
    private Vector2 offset;

    void Start()
    {
        hiddenPos = new Vector2(-10000, -10000);
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

    public void Show(string interactMessage, Transform target, Vector2 offset)
    {
        interactText.text = interactMessage;
        this.target = target;
        this.offset = offset;
        showing = true;
    }

    public void Hide()
    {
        transform.position = hiddenPos;
        showing = false;
    }
}
