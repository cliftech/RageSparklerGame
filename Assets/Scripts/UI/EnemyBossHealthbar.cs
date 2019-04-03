using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBossHealthbar : MonoBehaviour
{
    private Image healthbarFill;
    private Vector3 showingPos;
    private Vector3 hiddenPos;
    void Awake()
    {
        healthbarFill = transform.Find("Fill").GetComponent<Image>();
    }
    void Start()
    {
        showingPos = transform.position;
        hiddenPos = new Vector3(-10000, -10000);
        Hide();
    }

    public void Show()
    {
        transform.position = showingPos;
    }

    public void Hide()
    {
        transform.position = hiddenPos;
    }

    public void UpdateHealthbar(float health, float maxHealth)
    {
        healthbarFill.fillAmount = health / maxHealth;
    }
}
