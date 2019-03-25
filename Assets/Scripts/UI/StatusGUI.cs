using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusGUI : MonoBehaviour
{
    public Image healthBarFill;
    public Text essenceText;
    public Text levelText;
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
}
