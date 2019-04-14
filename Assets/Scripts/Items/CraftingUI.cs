using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public void OnCraftButtonClick()
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
                craftWindow.equipment.ShowToolTip(craftWindow.craftingRecipeUIs[i].slots[0]);
                break;
            }
        }
    }

    public void UpdateOneCraftingRecipe()
    {
        if (!recipe.CanCraft(Equipment, HubChest))
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
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
