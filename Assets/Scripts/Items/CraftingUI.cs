using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] RectTransform arrowParent;

    public Slot[] slots;
    public Inventory HubChest;
    public Inventory Equipment;

    private CraftingWindow craftWindow;
    private bool force = false;
    private CraftingRecipe recipe;
    public CraftingRecipe Recipe
    {
        get { return recipe; }
        set { SetCraftingRecipe(value, force); }
    }

    private void OnValidate()
    {
        slots = GetComponentsInChildren<Slot>(includeInactive: true);
    }

    public void OnCraftButtonClick(CraftingRecipe recipe)
    {
        recipe.Craft(Equipment, HubChest);
        UpdateAllCraftingRecipes();
    }

    private void Awake()
    {
        craftWindow = FindObjectOfType<CraftingWindow>();
    }

    public void Start()
    {
        UpdateOneCraftingRecipe();
    }

    public void UpdateAllCraftingRecipes()
    {
        for (int i = 0; i < craftWindow.craftingRecipeUIs.Count; i++)
        {
            craftWindow.craftingRecipeUIs[i].UpdateOneCraftingRecipe();
        }

        for (int i = 0; i < craftWindow.craftingRecipeUIs.Count; i++)
        {
            if (craftWindow.craftingRecipeUIs[i].gameObject.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(craftWindow.craftingRecipeUIs[i].slots[0].gameObject);
                break;
            }
        }
    }

    public void UpdateOneCraftingRecipe()
    {
        int count = 0;
        if (!this.Recipe.CanCraft(Equipment, HubChest) && !craftWindow.showAll)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);
            for (int i = 0; i < recipe.Materials.Count; i++)
            {
                slots[i].amountText.text = recipe.Materials[i].amount.ToString()+"/"+ recipe.CountAmount(Equipment, HubChest, recipe.Materials[i]).ToString();
                if (recipe.CountAmount(Equipment, HubChest, recipe.Materials[i]) < recipe.Materials[i].amount)
                {
                    slots[i].amountText.color = Color.red;
                    count++;
                }
                else
                {
                    slots[i].amountText.color = Color.white;
                }
            }
            if (count > 0)
            {
                slots[slots.Length - 1].GetComponentInChildren<Text>().color = Color.red;
                slots[slots.Length - 1].GetComponent<Image>().color = Color.black;
            }
            else
            {
                slots[slots.Length - 1].GetComponentInChildren<Text>().color = Color.white;
                slots[slots.Length - 1].GetComponent<Image>().color = Color.white;
            }
        }
    }

    private void SetCraftingRecipe(CraftingRecipe newCraftingRecipe, bool force)
    {
        if(!force)
        {
            Inventory[] equipments = FindObjectsOfType<Inventory>();
            for (int i = 0; i < equipments.Length; i++)
            {
                if (equipments[i].inventoryUI.name == "EquipmentUI")
                    Equipment = equipments[i];
                else
                    HubChest = equipments[i];
            }
            force = true;
        }
        recipe = newCraftingRecipe;

        if(recipe != null)
        {
            int slotIndex = 0;
            slotIndex = SetSlots(recipe.Materials, slotIndex);
            arrowParent.SetSiblingIndex(slotIndex);
            slotIndex = SetSlots(recipe.Results, slotIndex);

            for (int i = slotIndex; i < slots.Length; i++)
            {
                slots[i].transform.parent.gameObject.SetActive(false);
            }

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }    

    private int SetSlots(IList<ItemAmount> itemAmountList, int slotIndex)
    {
        for (int i = 0; i < itemAmountList.Count; i++, slotIndex++)
        {
            ItemAmount itemAmount = itemAmountList[i];
            Slot itemSlot = slots[slotIndex];

            itemSlot.itemas = itemAmount.item;
            itemSlot.Amount = itemAmount.amount;
            itemSlot.transform.parent.gameObject.SetActive(true);
        }
        return slotIndex;
    }
}
