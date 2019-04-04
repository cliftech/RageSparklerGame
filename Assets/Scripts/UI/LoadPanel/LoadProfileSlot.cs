﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadProfileSlot : MonoBehaviour
{
    public Text countText;
    public Text lvlText;
    public Text essenceText;
    public Text timePlayedText;
    public Button button;

    public void Set(int index, SaveProfile profile, LoadPanel panel)
    {
        countText.text = index.ToString();
        lvlText.text = profile.lvl.ToString();
        essenceText.text = profile.lvl.ToString();
        timePlayedText.text = GetFormattedTime(profile.timePlayed);
        button.onClick.AddListener(() => panel.LoadSlot(profile));
    }

    private string GetFormattedTime(float time)
    {
        int minutes = (int)time / 60;
        int hours = minutes / 60;
        minutes = minutes % 60;
        return string.Format("{0}H {1}M", hours, minutes);
    }
}