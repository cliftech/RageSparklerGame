using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private string diffDmg;
    private string diffArm;
    private string diffHp;

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
    public int whichSlot;

    public PlayerInteract inter;

    public float damage;
    public float armor;
    public float health;

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
        if (Input.GetButtonDown("Swap") && selected && !empty && gameObject.name.StartsWith("Slot"))
        { 
            inter.swap(item, ID, type, description, quality, itemName, icon, damage, armor, health, whichSlot);
        }
    }

    void Start()
    {
        slotIcon = transform.GetChild(0);
        diffDmg = string.Empty;
        diffArm = string.Empty;
        diffHp = string.Empty;
    }

    public void CompareItems(Slot compare)
    {
        diffDmg = string.Empty;
        diffArm = string.Empty;
        diffHp = string.Empty;


        if (damage > compare.damage)
            diffDmg = string.Format("<color=lime> (+{0})</color>", (damage - compare.damage).ToString());
        else if (damage < compare.damage)
            diffDmg = string.Format("<color=red> ({0})</color>", (damage - compare.damage).ToString());

        if (armor > compare.armor)
            diffArm = string.Format("<color=lime> (+{0})</color>", (armor - compare.armor).ToString());
        else if (armor < compare.armor)
            diffArm = string.Format("<color=red> ({0})</color>", (armor - compare.armor).ToString());

        if (health > compare.health)
            diffHp = string.Format("<color=lime> (+{0})</color>", (health - compare.health).ToString());
        else if (health < compare.health)
            diffHp = string.Format("<color=red> ({0})</color>", (health - compare.health).ToString());
    }

    public string GetToolTip(bool compare)
    {
        string comparison = string.Empty;
        string stats = string.Empty;
        string color = string.Empty;
        string newLine = string.Empty;

        if (compare)
            comparison = "Currently equiped:\n";

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
            stats += "\n+" + damage.ToString() + " Damage" + diffDmg;
        }
        if (armor > 0)
        {
            stats += "\n+" + armor.ToString() + " Armor" + diffArm;
        }
        if (health > 0)
        {
            stats += "\n+" + health.ToString() + " Health" + diffHp;
        }
        return string.Format("<color=black><size=10>" + comparison + "</size></color><color=" + color + "><size=16>{0}</size></color><size=14>{1}<i><color=lime>" + newLine + "{2}</color></i></size>", itemName, stats, description);
    }
}
