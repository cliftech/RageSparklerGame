using UnityEditor;
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
    public GameObject Object2D;
    public Quality quality;
    [SerializeField] string id;
    public string ID { get { return id; } }
    public string itemName;
    public string description;
    public Sprite icon;
    public bool pickedUp;
    public bool equipable;
    [Space]
    public float damage;
    public float armor;
    public float health;
    [Space]
    public ItemType type;
    [Space]
    [Range(0, 100)]
    public float healPercent;

    private void OnValidate()
    {
        string path = AssetDatabase.GetAssetPath(this);
        id = AssetDatabase.AssetPathToGUID(path);
    }
}