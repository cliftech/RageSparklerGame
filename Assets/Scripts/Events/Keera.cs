using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keera : MonoBehaviour
{
    public string displayName = "Keera";
    public List<string> dialogLines;
    public int lineIndex;
    private DialogBox dialogBox;
    private bool allLinesShown;

    public void Awake()
    {
        dialogBox = FindObjectOfType<DialogBox>();
    }

    void Start()
    {
        lineIndex = 0;
        ShowNextLine();
    }

    void Update()
    {
        if(Input.GetButtonDown("Interact"))
        {
            if (!allLinesShown)
                ShowNextLine();
            else
            {
                dialogBox.HideText();
                this.enabled = false;
            }
        }
    }

    private void ShowNextLine()
    {
        dialogBox.ShowText(displayName, dialogLines[lineIndex], -1, true);
        lineIndex++;
        if(lineIndex >= dialogLines.Count)
            allLinesShown = true;
    }
}
