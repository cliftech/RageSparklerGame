using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusGUI : MonoBehaviour
{
    public Image healthBarFill;
    public Text essenceText;
    public Text levelText;
    public Text armorIconText;
    public Text healthIconText;
    public Text damageIconText;
    private Player player;
    void Awake()
    {
        player = FindObjectOfType<Player>();
    }
    public void UpdateHealthbar()
    {
        healthBarFill.fillAmount = player.Health / player.activeMaxHealth;
    }
    public void UpdateEssenceText()
    {
        essenceText.text = player.essence.ToString();
    }
    public void UpdateLevelText()
    {
        levelText.text = player.level.ToString();
    }
    public void UpdateInventoryStats()
    {
        armorIconText.text = player.Armor.ToString("0");
        healthIconText.text = player.activeMaxHealth.ToString("0");
        damageIconText.text = player.attack1Dam.ToString("0");
    }
}
