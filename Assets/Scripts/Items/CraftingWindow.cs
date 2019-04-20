using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftingWindow : MonoBehaviour
{
    [SerializeField] CraftingUI recipeUIPrefab;
    [SerializeField] RectTransform recipeUIParent;

    public List<CraftingUI> craftingRecipeUIs;
    public Inventory hubChest;
    public Inventory equipment;

    public List<CraftingRecipe> craftingRecipes;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        recipeUIParent.GetComponentsInChildren<CraftingUI>(includeInactive: true, result: craftingRecipeUIs);
        UpdateCraftingRecipes();
    }

    public void UpdateCraftingRecipes()
    {
        for (int i = 0; i < craftingRecipes.Count; i++)
        {
            if(craftingRecipeUIs.Count == i)
            {
                craftingRecipeUIs.Add(Instantiate(recipeUIPrefab, recipeUIParent, false));
            }
            else if(craftingRecipeUIs[i] == null)
            {
                craftingRecipeUIs[i] = Instantiate(recipeUIPrefab, recipeUIParent, false);
            }

            craftingRecipeUIs[i].Equipment = equipment;
            craftingRecipeUIs[i].HubChest = hubChest;
            craftingRecipeUIs[i].Recipe = craftingRecipes[i];
        }

        for (int i = craftingRecipes.Count; i < craftingRecipeUIs.Count; i++)
        {
            craftingRecipeUIs[i].Recipe = null;
        }
    }

    public void FindGrey()
    {
        Transform panel;
        for (int i = 0; i < craftingRecipeUIs.Count; i++)
        {
            for (int j = 0; j < craftingRecipeUIs[i].slots.Length; j++)
            {
                panel = craftingRecipeUIs[i].slots[j].transform.GetChild(0);
                if (panel.GetComponent<Image>().color == Color.grey)
                {
                    panel.GetComponent<Image>().color = Color.white;
                }
            }          
        }
    }
}
