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
    public string description;
    public Sprite icon;
    public Sprite defaultIcon;
    public Transform slotIcon;

    public PlayerInteract inter;

    public void UpdateSlot()
    {
        if(icon != null)
            slotIcon.GetComponent<Image>().sprite = icon;
        else
            slotIcon.GetComponent<Image>().sprite = defaultIcon;
    }

    public void Swap()
    {
        if(!empty)
            inter.swap(item, ID, type, description, icon);
    }

    private void Start()
    {
        slotIcon = transform.GetChild(0);
    }

}
