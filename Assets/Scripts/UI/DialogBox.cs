using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    public TextManager textManager;
    public Text speakerText;
    public RectTransform canvasRect;
    private RectTransform rect;
    public string playerName = "42";
    public Image keyIcon;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        if(textManager.isEmpty())
            HideText();
    }

    public void ShowText(string speakerName, string text, float timeToShow = 5, bool showKeyIcon = false)
    {
        gameObject.SetActive(true);
        if (rect == null)
            rect = GetComponent<RectTransform>();
        textManager.SetActionOnceShown(() => AllTextIsShown());
        textManager.ShowAllText(text);
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
        textManager.ClearText();
        gameObject.SetActive(false);
    }

    private void AllTextIsShown() {

    }

    private IEnumerator HideAfter(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        HideText();
    }
}
