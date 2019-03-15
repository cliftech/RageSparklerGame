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
    public Transform slotIconGO;

    public void UpdateSlot()
    {
        slotIconGO.GetComponent<Image>().sprite = icon;
    }

    private void Start()
    {
        slotIconGO = transform.GetChild(0);
    }
}
