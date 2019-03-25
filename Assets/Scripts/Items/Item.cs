using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Item : MonoBehaviour
{
    public int ID;
    public string type;
    public string itemName;
    public string description;
    public Sprite icon;
    public string quality;
    public bool pickedUp;
    public bool equipable;

    public float damage;
    public float armor;
    public float health;
}
