using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBossHealthbar : MonoBehaviour
{
    private Image healthbarFill;
    void Awake()
    {
        healthbarFill = transform.Find("Fill").GetComponent<Image>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (healthbarFill == null)
            Awake();

    }

    public void Hide()
    {
        print("shoinwg");
        gameObject.SetActive(false);
    }

    public void UpdateHealthbar(float health, float maxHealth)
    {
        healthbarFill.fillAmount = health / maxHealth;
    }
}
