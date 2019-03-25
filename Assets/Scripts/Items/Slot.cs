using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private Navigation navigation;
    private Button button;

    public bool selected = false;
    public GameObject item;
    public bool empty;
    public int ID;
    public string quality;
    public string type;
    public string itemName;
    public string description;
    public Sprite icon;
    public Sprite defaultIcon;
    public Transform slotIcon;

    public PlayerInteract inter;

    public float damage;
    public float armor;

    public void UpdateSlot()
    {
        if (icon != null)
            slotIcon.GetComponent<Image>().sprite = icon;
        else
        {
            slotIcon.GetComponent<Image>().sprite = defaultIcon;
            slotIcon.GetComponent<Image>().color = Color.white;
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && selected && !empty)
        {
            inter.swap(item, ID, type, description, quality, itemName, icon);
        }
    }

    void Start()
    {
        slotIcon = transform.GetChild(0);
    }

    public string GetToolTip()
    {
        string stats = string.Empty;
        string color = string.Empty;
        string newLine = string.Empty;

        if (description != string.Empty)
        {
            newLine = "\n";
        }

        if (quality == "Common")
            color = "white";
        else if (quality == "Rare")
            color = "navy";
        else if (quality == "Epic")
            color = "magenta";
        else if (quality == "Legendary")
            color = "orange";

        if (damage > 0)
        {
            stats += "\n+" + damage.ToString() + " Damage";
        }
        if (damage > 0)
        {
            stats += "\n+" + armor.ToString() + " Damage";
        }
        return string.Format("<color=" + color + "><size=16>{0}</size></color><size=14><i><color=lime>" + newLine + "{1}</color></i>{2}</size>", itemName, description, stats);
    }
}
