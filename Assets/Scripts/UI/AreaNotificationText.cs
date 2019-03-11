using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaNotificationText : MonoBehaviour
{
    private Text text;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    void Start()
    {
        Color c = text.color;
        c.a = 0;
        text.color = c;
    }

    public void ShowNotification(string s, float fadeInTime = 1f, float fadeOutTime = 2f)
    {
        text.text = s;
        StopAllCoroutines();
        StartCoroutine(FadeIn(fadeInTime, fadeOutTime));
    }

    private IEnumerator FadeIn(float fadeInTime, float fadeOutTime)
    {
        Color c = text.color;
        c.a = 0;
        text.color = c;
        float time = 0;
        while (text.color.a < 0.9f)
        {
            c = text.color;
            c.a = Mathf.Lerp(c.a, 1, Time.fixedDeltaTime / fadeInTime);
            text.color = c;
            yield return new WaitForFixedUpdate();
        }
        c = text.color;
        c.a = 1f;
        text.color = c;
        yield return new WaitForSecondsRealtime(Mathf.Clamp(fadeInTime - time, 0, fadeInTime));
        StartCoroutine(FadeOut(fadeOutTime));
    }
    private IEnumerator FadeOut(float time)
    {
        Color c = text.color;
        c.a = 1f;
        text.color = c;
        while (text.color.a > 0.001f)
        {
            c = text.color;
            c.a = Mathf.MoveTowards(c.a, 0, Time.fixedDeltaTime / time);
            text.color = c;
            yield return new WaitForFixedUpdate();
        }
        c = text.color;
        c.a = 0;
        text.color = c;
    }
}
