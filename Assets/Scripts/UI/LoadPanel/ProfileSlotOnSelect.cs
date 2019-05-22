using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProfileSlotOnSelect : MonoBehaviour, ISelectHandler
{
    public LoadPanel loadPanel;
    private int index;

    public void Set(int index, LoadPanel panel)
    {
        loadPanel = panel;
        this.index = index;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (index != -1)
            loadPanel.MoveProfileEditButtons(index);
        else
            loadPanel.HideProfileEditButtons();
    }
}
