using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public Text text;
    public Text speakerText;
    public RectTransform canvasRect;
    private RectTransform rect;
    public string playerName = "42";
    public Image keyIcon;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        HideText();
    }

    public void ShowText(string speakerName, string text, float timeToShow = 5, bool showKeyIcon = false)
    {
        gameObject.SetActive(true);
        if (rect == null)
            Start();
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
        gameObject.SetActive(false);
    }

    private IEnumerator HideAfter(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        HideText();
    }
}
