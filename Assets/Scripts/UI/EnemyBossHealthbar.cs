using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBossHealthbar : MonoBehaviour
{
    private Image healthbarFill;
    private Text bossName;
    void Awake()
    {
        healthbarFill = transform.Find("Fill").GetComponent<Image>();
        bossName = transform.Find("Boss Name").GetComponent<Text>();
    }

    public void Show(string bossName = "")
    {
        gameObject.SetActive(true);
        if (healthbarFill == null)
            Awake();
        this.bossName.text = bossName;

    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateHealthbar(float health, float maxHealth)
    {
        healthbarFill.fillAmount = health / maxHealth;
    }
}
