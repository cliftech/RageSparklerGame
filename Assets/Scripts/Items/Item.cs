﻿using UnityEditor;
using UnityEngine;

public enum Quality
{ Common, Rare, Epic, Legendary }

public enum ItemType
{
    Helmet, Amulet, BodyArmor, Weapon, LegArmor, Boots, Gloves, SecondaryWeapon,
    Potions, Ring, Cape, Material
}

[CreateAssetMenu]
public class Item : ScriptableObject
{
    [Header("[Required attributes for all items]")]
    [Header("Don't forget to add item to item database !")]
    public GameObject Object2D;
    public Quality quality;
    public bool equipable;
    public Sprite icon;
    public string itemName;
    public ItemType type;
    [Range(1,999)]
    public int MaximumStack = 1;
    [Header("Equipement stats")]
    [Header("[Optional attribues]")]
    public float damage;
    public float armor;
    public float health;
    [Space]
    [Header("Potion stats")]
    public Sprite Empty;
    public Sprite TwoThirds;
    public Sprite OneThird;
    public Sprite Full;
    [Range(0, 100)]
    public float healPercent;
    public int maxUses;
    public int currentUses;
    [Space]
    [Header("Other")]
    public string description;
    [HideInInspector]
    public string ID;
}