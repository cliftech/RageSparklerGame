using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] RectTransform arrowParent;

    public Slot[] slots;
    public Inventory HubChest;
    public Inventory Equipment;

    private CraftingRecipe recipe;
    public CraftingRecipe Recipe
    {
        get { return recipe; }
        set { SetCraftingRecipe(value); }
    }

    private void Awake()
    {
        Inventory[] equipments = GetComponents<Inventory>();
        for (int i = 0; i < equipments.Length; i++)
        {
            if (equipments[i].inventoryUI.name == "EquipmentUI")
                Equipment = equipments[i];
            else
                HubChest = equipments[i];
        }
    }

    private void OnValidate()
    {
        slots = GetComponentsInChildren<Slot>(includeInactive: true);
    }

    public void OnCraftButtonClick()
    {
        recipe.Craft(Equipment, HubChest);
    }

    private void SetCraftingRecipe(CraftingRecipe newCraftingRecipe)
    {
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
