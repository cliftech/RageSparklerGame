using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCover : MonoBehaviour
{
    public RectTransform canvasRect;
    private RectTransform rect;
    private Image screenCover;

    private Action coveredAction;
    private Vector2 hiddenPos;
    private Vector2 centerPos;

    private void Awake()
    {
        screenCover = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }
    private void Start()
    {
        hiddenPos = new Vector2(-10000, -10000);
        // Calculate *screen* position (note, not a canvas/recttransform position)
        Vector2 centerScreenPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, centerScreenPoint, null, out centerPos);
    }

    public void CoverScreen(float time, Action coveredAction)
    {
        StopAllCoroutines();
        this.coveredAction = coveredAction;
        rect.anchoredPosition = centerPos;
        StartCoroutine(CoverScreen_Routine(time));
    }

    public void UncoverScreen(float time)
    {
        StopAllCoroutines();
        rect.anchoredPosition = centerPos;
        StartCoroutine(UncoverScreen_Routine(time));
    }

    private IEnumerator CoverScreen_Routine(float fadeInTime)
    {
        Color c = screenCover.color;
        c.a = 0;
        screenCover.color = c;
        while (screenCover.color.a < 0.9f)
        {
            c = screenCover.color;
            c.a = Mathf.Lerp(c.a, 1, Time.fixedDeltaTime / fadeInTime);
            screenCover.color = c;
            yield return new WaitForFixedUpdate();
        }
        c = screenCover.color;
        c.a = 1f;
        screenCover.color = c;
        yield return new WaitForFixedUpdate();
        coveredAction.Invoke();
    }
    private IEnumerator UncoverScreen_Routine(float time)
    {
        Color c = screenCover.color;
        c.a = 1f;
        screenCover.color = c;
        while (screenCover.color.a > 0.01f)
        {
            c = screenCover.color;
            c.a = Mathf.Lerp(c.a, 0, Time.fixedDeltaTime / time);
            screenCover.color = c;
            yield return new WaitForFixedUpdate();
        }
        c = screenCover.color;
        c.a = 0;
        screenCover.color = c;
        rect.anchoredPosition = hiddenPos;
    }
}
