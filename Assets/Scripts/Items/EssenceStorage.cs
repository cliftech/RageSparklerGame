using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssenceStorage : MonoBehaviour
{
    public int esssenceStoraged;

    public Sprite empty;
    public Sprite quarter;
    public Sprite half;
    public Sprite threeQuarters;
    public Sprite full;

    void UpdateRenderer()
    {
        if (esssenceStoraged < 10)
            GameObject.Find("EssenceStorage").GetComponent<SpriteRenderer>().sprite = empty;
        else if (esssenceStoraged >= 10 && esssenceStoraged < 20)
            GameObject.Find("EssenceStorage").GetComponent<SpriteRenderer>().sprite = quarter;
        else if (esssenceStoraged >= 20 && esssenceStoraged < 30)
            GameObject.Find("EssenceStorage").GetComponent<SpriteRenderer>().sprite = half;
        else if (esssenceStoraged >= 30 && esssenceStoraged < 40)
            GameObject.Find("EssenceStorage").GetComponent<SpriteRenderer>().sprite = threeQuarters;
        else if (esssenceStoraged >= 40)
            GameObject.Find("EssenceStorage").GetComponent<SpriteRenderer>().sprite = full;
    }

    public void StoreEssence(int amount)
    {
        esssenceStoraged += amount;
        UpdateRenderer();
    }

    public int TakeEssence()
    {
        int returnAmount = esssenceStoraged;
        esssenceStoraged = 0;
        UpdateRenderer();
        return returnAmount;
    }
}
