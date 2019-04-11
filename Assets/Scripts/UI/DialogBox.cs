using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public Text text;
    public Text speakerText;
    private Vector2 hiddenPosition;
    private Vector2 showingPosition;
    public RectTransform canvasRect;
    private RectTransform rect;
    public string playerName = "42";
    public Image keyIcon;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        hiddenPosition = new Vector2(-10000, -10000);
        showingPosition = rect.anchoredPosition;
        HideText();
    }

    public void ShowText(string speakerName, string text, float timeToShow = 5, bool showKeyIcon = false)
    {
        rect.anchoredPosition = showingPosition;
        this.text.text = text;
        speakerText.text = speakerName;
        keyIcon.enabled = showKeyIcon;
        if(timeToShow > 0)
        {
            StopAllCoroutines();
            StartCoroutine(HideAfter(timeToShow));
        }
    }

    public void HideText()
    {
        rect.anchoredPosition = hiddenPosition;
    }

    private IEnumerator HideAfter(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        HideText();
    }
}
