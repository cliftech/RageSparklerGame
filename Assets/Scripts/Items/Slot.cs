using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    private Player plr;

    private string diffDmg;
    private string diffArm;
    private string diffHp;

    private string once = null;

    private CraftingWindow craftWinow;

    public ItemType equipType;

    [SerializeField] Item _itemas;
    public CraftingRecipe craftRecipe;
    public Text amountText;
    public Item itemas
    {
        get { return _itemas; }
        set
        {
            _itemas = value;

            if (_itemas == null)
            {
                slotIcon = transform.GetChild(0);
                slotIcon.GetComponent<Image>().sprite = null;
                slotIcon.GetComponent<Image>().sprite = defaultIcon;
                slotIcon.GetComponent<Image>().color = Color.white;
            }
            else
            {
                slotIcon = transform.GetChild(0);
                slotIcon.GetComponent<Image>().sprite = null;
                slotIcon.GetComponent<Image>().sprite = _itemas.icon;
            }
        }
    }

    private int _amount;
    public int Amount
    {
        get { return _amount; }
        set
        {
            _amount = value;
            if (_amount < 0)
                _amount = 0;
            if (_amount == 0 && itemas != null)
                itemas = null;

            if (amountText != null)
            {
                amountText.enabled = _itemas != null && name == "Item Slot" || _amount > 1;
                if (amountText.enabled)
                {
                    amountText.text = _amount.ToString();
                }
            }
        }
    }

    public bool selected = false;
    public Sprite icon;
    public Sprite defaultIcon;
    public Transform slotIcon;
    public int whichSlot;

    public PlayerInteract inter;

    void Update()
    {
        if (Input.GetButtonDown("Swap") && selected && itemas != null && gameObject.name.StartsWith("Slot") && itemas.type.ToString() != "Material")
        {
            inter.swap(itemas, whichSlot);
            plr.statusGUI.UpdatePotionCharges();
        }
        if (Input.GetButtonDown("Swap") && selected && itemas != null && !gameObject.name.StartsWith("Slot") && !gameObject.name.StartsWith("Item"))
        {
            inter.Unequip(itemas, this);
            plr.statusGUI.UpdatePotionCharges();
        }
        if (Input.GetButtonDown("Swap") && selected && gameObject.name == "CraftButton" && once == null)
        {
            once = craftRecipe.name;
            this.GetComponentInParent<CraftingUI>().OnCraftButtonClick(craftRecipe);
        }
    }

    private void LateUpdate()
    {
        once = null;
    }

    private void Awake()
    {
        plr = FindObjectOfType<Player>();
        craftWinow = FindObjectOfType<CraftingWindow>();
    }

    void Start()
    {
        slotIcon = transform.GetChild(0);
        if (amountText == null)
        {
            amountText = GetComponentInChildren<Text>();
        }
        diffDmg = string.Empty;
        diffArm = string.Empty;
        diffHp = string.Empty;
    }

    public void Select()
    {
        selected = true;
    }

    public void Deselect()
    {
        selected = false;
    }

    public void CompareItems(Slot compare)
    {
        diffDmg = string.Empty;
        diffArm = string.Empty;
        diffHp = string.Empty;


        if (itemas.damage > compare.itemas.damage)
            diffDmg = string.Format("<color=lime> (+{0})</color>", (itemas.damage - compare.itemas.damage).ToString());
        else if (itemas.damage < compare.itemas.damage)
            diffDmg = string.Format("<color=red> ({0})</color>", (itemas.damage - compare.itemas.damage).ToString());

        if (itemas.armor > compare.itemas.armor)
            diffArm = string.Format("<color=lime> (+{0})</color>", (itemas.armor - compare.itemas.armor).ToString());
        else if (itemas.armor < compare.itemas.armor)
            diffArm = string.Format("<color=red> ({0})</color>", (itemas.armor - compare.itemas.armor).ToString());

        if (itemas.health > compare.itemas.health)
            diffHp = string.Format("<color=lime> (+{0})</color>", (itemas.health - compare.itemas.health).ToString());
        else if (itemas.health < compare.itemas.health)
            diffHp = string.Format("<color=red> ({0})</color>", (itemas.health - compare.itemas.health).ToString());
    }

    public string GetToolTip(bool compare)
    {
        string comparison = string.Empty;
        string stats = string.Empty;
        string color = string.Empty;
        string newLine = string.Empty;

        if (compare)
            comparison = "Currently equiped:\n";

        if (itemas.description != string.Empty)
        {
            newLine = "\n";
        }

        if (itemas.quality.ToString() == "Common")
            color = "white";
        else if (itemas.quality.ToString() == "Rare")
            color = "navy";
        else if (itemas.quality.ToString() == "Epic")
            color = "magenta";
        else if (itemas.quality.ToString() == "Legendary")
            color = "orange";

        if (itemas.damage > 0)
        {
            stats += "\n+" + itemas.damage.ToString() + " Damage" + diffDmg;
        }
        if (itemas.armor > 0)
        {
            stats += "\n+" + itemas.armor.ToString() + " Armor" + diffArm;
        }
        if (itemas.health > 0)
        {
            stats += "\n+" + itemas.health.ToString() + " Health" + diffHp;
        }
        return string.Format("<color=black><size=10>" + comparison + "</size></color><color=" + color + "><size=16>{0}</size></color><size=14>{1}<i><color=lime>" + newLine + "{2}</color></i></size>", itemas.itemName, stats, itemas.description);
    }
}
