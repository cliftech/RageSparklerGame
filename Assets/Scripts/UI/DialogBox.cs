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

    void Start()
    {
        rect = GetComponent<RectTransform>();
        hiddenPosition = new Vector2(-10000, -10000);
        showingPosition = rect.anchoredPosition;
        HideText();
    }

    void Update()
    {
        
    }

    public void ShowText(string speakerName, string text)
    {
        rect.anchoredPosition = showingPosition;
        this.text.text = text;
        speakerText.text = speakerName;
        float timeToShow = 5;
        StopAllCoroutines();
        StartCoroutine(HideAfter(timeToShow));
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
