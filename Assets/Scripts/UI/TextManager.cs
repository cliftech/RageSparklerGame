using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TextManager : MonoBehaviour
{
    public AudioClip charAppearSound;
    private AudioSource audioSource;

    private Text text;

    private float textCharAppearFrequency = 0.03f;
    private float textIntaAppearFrequency = 0;
    private float actualFrequency;

    private Action actionOnceShown;

    void Awake()
    {
        text = GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = charAppearSound;
    }

    public void SetActionOnceShown(Action action)
    {
        actionOnceShown = action;
    }

    private IEnumerator AddOneCharAtATime(string line)
    {
        string textBefore = text.text;
        string emptyLine = new string(' ', line.Length - 1);

        for (int i = 0; i < line.Length - 1; i++)
        {
            emptyLine = emptyLine.Remove(i, 1);
            emptyLine = emptyLine.Insert(i, line[i].ToString());
            text.text = textBefore + emptyLine;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(actualFrequency);
        }
        text.text = textBefore + line;
        actionOnceShown.Invoke();
    }

    public void AddLineOfText(string line)
    {
        actualFrequency = textCharAppearFrequency;
        StartCoroutine(AddOneCharAtATime(line));
    }

    private IEnumerator ShowAllTextOneCharAtATime(string text) {
        foreach (char c in text) {
            this.text.text += c;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(actualFrequency);
        }
        actionOnceShown.Invoke();
    }

    public void ShowAllText(string text) {
        StopAllCoroutines();
        this.text.text = "";
        actualFrequency = textCharAppearFrequency;
        StartCoroutine(ShowAllTextOneCharAtATime(text));
    }

    public void TextInstantAppear()
    {
        actualFrequency = textIntaAppearFrequency;
    }

    public void ClearText()
    {
        text.text = "";
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void Disable()
    {
        StopAllCoroutines();
        this.enabled = false;
    }

    public bool isEmpty()
    {
        return text.text == "";
    }
}
