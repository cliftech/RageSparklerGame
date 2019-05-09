using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableGUI : MonoBehaviour
{
    public RectTransform canvasRect;
    public Text interactText;
    public Text secondaryText;
    public Image icon;
    public Sprite essenceIcon;

    private bool showing;
    private Transform target;
    private Vector2 offset;

    void Start()
    {
        if(target == null)
            Hide();
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

    public void Show(string interactMessage, Transform target, Vector2 offset, string secondaryText = "", string iconToShow = "")
    {
        gameObject.SetActive(true);
        interactText.text = interactMessage;
        this.target = target;
        this.offset = offset;
        showing = true;

        if (secondaryText != "" && iconToShow != "")
        {
            icon.gameObject.SetActive(true);
            this.secondaryText.text = secondaryText;
            if (iconToShow == "essence")
                icon.sprite = essenceIcon;
        }
        else
            icon.gameObject.SetActive(false);
    }

    public void Hide()
    {
        showing = false;
        gameObject.SetActive(false);
    }
}
