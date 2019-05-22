using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadProfileSlot : MonoBehaviour
{
    private LoadPanel loadPanel;
    public Text countText;
    public Text lvlText;
    public Text essenceText;
    public Text timePlayedText;
    public Button button;
    private int index;

    public void Set(int index, SaveProfile profile, LoadPanel panel)
    {
        this.loadPanel = panel;
        this.index = index;
        countText.text = index.ToString();
        lvlText.text = profile.lvl.ToString();
        essenceText.text = profile.essence.ToString();
        timePlayedText.text = GetFormattedTime(profile.timePlayed);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => panel.LoadSlot(profile));

        transform.Find("Button").GetComponent<ProfileSlotOnSelect>().Set(index, panel);
    }

    public void SetNull(int index, LoadPanel panel)
    {
        countText.text = index.ToString();
        lvlText.text = "-";
        essenceText.text = "-";
        timePlayedText.text = "-H -M";
        button.onClick.RemoveAllListeners();

        transform.Find("Button").GetComponent<ProfileSlotOnSelect>().Set(-1, panel);
    }

    private string GetFormattedTime(float time)
    {
        int minutes = (int)time / 60;
        int hours = minutes / 60;
        minutes = minutes % 60;
        return string.Format("{0}H {1}M", hours, minutes);
    }
}
