using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class PortalGUI : MonoBehaviour
{
    private EventSystem eventSystem;
    public RectTransform canvasRect;
    private RectTransform rect;

    private bool showing;
    private Transform target;
    private Vector2 offset;

    public GameObject option;
    public int optionsToGenerate;
    private List<Button> optionButtons;
    private List<Text> optionTexts;
    private float buttonHeight;

    void Awake()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        optionButtons = new List<Button>();
        optionTexts = new List<Text>();
        rect = GetComponent<RectTransform>();

        Button b = option.GetComponent<Button>();
        Text t = b.GetComponentInChildren<Text>();
        RectTransform r = b.GetComponent<RectTransform>();
        optionButtons.Add(b);
        optionTexts.Add(t);
        buttonHeight = r.sizeDelta.y;
        for (int i = 0; i < optionsToGenerate - 1; i++)
        {
            b = Instantiate(b.gameObject, b.transform.position, Quaternion.identity, transform).GetComponent<Button>();
            t = b.GetComponentInChildren<Text>();
            r = b.GetComponent<RectTransform>();
            Vector2 p = r.anchoredPosition;
            p.y -= buttonHeight;
            r.anchoredPosition = p;

            optionButtons.Add(b);
            optionTexts.Add(t);
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

    public void Show(Transform target, Vector2 offset)
    {
        gameObject.SetActive(true);
        this.target = target;
        this.offset = offset;
        showing = true;
        UpdateGUI();
        eventSystem.SetSelectedGameObject(optionButtons[0].gameObject);
    }

    public void SetOption(int index, UnityEngine.Events.UnityAction onClickAction, string optionText)
    {
        optionButtons[index].gameObject.SetActive(true);
        optionButtons[index].onClick.RemoveAllListeners();
        optionButtons[index].onClick.AddListener(onClickAction);
        optionTexts[index].text = optionText;
    }

    public void Hide()
    {
        for (int i = 0; i < optionButtons.Count; i++)
            optionButtons[i].gameObject.SetActive(false);
        showing = false;
        gameObject.SetActive(false);
    }

    private void UpdateGUI()
    {
        int buttonsEnabled = 0;
        for (int i = 0; i < optionButtons.Count && optionButtons[i].gameObject.activeSelf; i++, buttonsEnabled++) ;

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, buttonHeight * buttonsEnabled);

        Navigation nav = optionButtons[0].navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnUp = optionButtons[buttonsEnabled - 1];
        nav.selectOnDown = buttonsEnabled > 0 ? optionButtons[1] : null;
        nav.selectOnLeft = null;
        nav.selectOnRight = null;
        optionButtons[0].navigation = nav;

        for (int i = 1; i < buttonsEnabled - 1; i++)
        {
            nav.selectOnUp = optionButtons[i - 1];
            nav.selectOnDown = optionButtons[i + 1];
            optionButtons[i].navigation = nav;
        }

        // if butt.enabled sniff it
        if (buttonsEnabled > 1)
            nav.selectOnUp = optionButtons[buttonsEnabled - 2];
        else
            nav.selectOnUp = null;
        nav.selectOnDown = optionButtons[0];
        optionButtons[buttonsEnabled - 1].navigation = nav;
    }
}
