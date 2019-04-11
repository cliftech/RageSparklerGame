using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackgroundStory : MonoBehaviour
{
    public Text text;
    public List<string> storyLines;
    public float intervalBetweenLines;
    private float timer;
    private int lineIndex;
    private bool allLinesShown;
    public float timeToShowAllLines;
  
    void Start()
    {
        lineIndex = 0;
        timer = 0;
        text.text = "";
    }

    
    void Update()
    {
        timer += Time.deltaTime;
        if (allLinesShown)
        {
            if(timer > timeToShowAllLines)
            {
                LoadSceneById(1);
                this.enabled = false;
            }
            return;
        }
        if(timer >= intervalBetweenLines)
        {
            ShowNextLine();
            timer = 0;
        }
    }

    private void ShowNextLine()
    {
        if (storyLines[lineIndex] == "F")
        {
            text.text = "";
            lineIndex++;
        }       
        text.text += storyLines[lineIndex] + "\n";
        lineIndex++;
        if(lineIndex >= storyLines.Count)
            allLinesShown = true;
    }

    public void LoadSceneById(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
