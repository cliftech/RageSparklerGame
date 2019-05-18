using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackgroundStory : MonoBehaviour
{
    private TextManager textManager;
    [TextArea(5, 50)]
    public string story;
    private string[] storyLines;
    private int lineIndex;
    private bool allLinesShown;

    private float pauseBetweenLines = 0.5f;

    private void Awake()
    {
        textManager = GetComponent<TextManager>();
    }

    void Start()
    {
        lineIndex = 0;
        storyLines = story.Split('\n');
        textManager.SetActionOnceShown(() => PreviousLineShow());
        textManager.ClearText();
        StartCoroutine(ShowNextLineWithDelay(1f));
    }

    private IEnumerator ShowNextLineWithDelay(float delay) {
        if (allLinesShown)
        {
            LoadSceneById(1);
            this.enabled = false;
            textManager.Disable();
        }
        else
        {
            yield return new WaitForSecondsRealtime(delay);
            ShowNextLine();
        }
    }

    private void PreviousLineShow() {
        StartCoroutine(ShowNextLineWithDelay(pauseBetweenLines));
    }

    
    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            LoadSceneById(1);
            this.enabled = false;
        }
    }

    private void ShowNextLine()
    {
        if (storyLines[lineIndex] == "F")
        {
            textManager.ClearText();
            lineIndex++;
        }
        textManager.AddLineOfText(storyLines[lineIndex] + "\n");
        lineIndex++;
        if(lineIndex >= storyLines.Length)
            allLinesShown = true;
    }

    public void LoadSceneById(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
