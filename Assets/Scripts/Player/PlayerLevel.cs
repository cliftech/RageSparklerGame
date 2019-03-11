using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    public Text levelText;
    public int currentLevel;


    public int[] xpToLevelUp;

    public void SetLevelText()
    {
        levelText.text = "Level : " + currentLevel.ToString();
    }
}
