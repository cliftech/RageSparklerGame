using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public GameObject item;
    public bool empty;
    public int ID;
    public string type;
    public string descriptio;
    public Sprite icon;
    public Sprite defaultIcon;
    public Transform slotIcon;

    public void UpdateSlot()
    {
        if(icon != null)
            slotIcon.GetComponent<Image>().sprite = icon;
        else
            slotIcon.GetComponent<Image>().sprite = defaultIcon;
    }

    private void Start()
    {
        slotIcon = transform.GetChild(0);
    }
}
