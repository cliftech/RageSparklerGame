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

    void Update()
    {
        if (esssenceStoraged < 10)
            this.gameObject.GetComponent<SpriteRenderer>().sprite = empty;
        else if (esssenceStoraged >= 10 && esssenceStoraged < 20)
            this.gameObject.GetComponent<SpriteRenderer>().sprite = quarter;
        else if (esssenceStoraged >= 20 && esssenceStoraged < 30)
            this.gameObject.GetComponent<SpriteRenderer>().sprite = half;
        else if (esssenceStoraged >= 30 && esssenceStoraged < 40)
            this.gameObject.GetComponent<SpriteRenderer>().sprite = threeQuarters;
        else if (esssenceStoraged >= 40)
            this.gameObject.GetComponent<SpriteRenderer>().sprite = full;
    }
    public void StoreEssence(int amount)
    {
        esssenceStoraged += amount;
    }

    public int TakeEssence()
    {
        int returnAmount = esssenceStoraged;
        esssenceStoraged = 0;
        return returnAmount;
    }
}
