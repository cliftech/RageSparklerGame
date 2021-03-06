﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EssenceCollector : MonoBehaviour
{
    private Transform fill;
    private Player player;
    private int maxEssence;
    private Vector3 guiOffset = new Vector3(0, 1, 0);
    private EssenceCollectorGUI collectorGUI;
    private GameManager gameManager;

    private int[] maxEssenceUpgrades = { 50, 100, 200, 500, 1000, 2000, 5000, 10000 };

    public void Initialize(int maxEssenceUpgrade)
    {
        collectorGUI = Resources.FindObjectsOfTypeAll<EssenceCollectorGUI>()[0];
        gameManager = FindObjectOfType<GameManager>();
        player = FindObjectOfType<Player>(); fill = transform.GetChild(0);
        this.maxEssence = maxEssenceUpgrades[maxEssenceUpgrade];
    }

    public void UpdateEssenceCollector()
    {
        fill.localScale = new Vector3(1, (float)player.storedEssence/maxEssence, 1);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            collectorGUI.Show(transform, guiOffset, () => Deposit(), () => Withdraw());
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            collectorGUI.Hide();
        }
    }
    private void Deposit()
    {
        int essenceTaken = (int)Mathf.Clamp(player.essence, 0, maxEssence - player.storedEssence);

        player.storedEssence += essenceTaken;
        player.essence -= essenceTaken;
        UpdateEssenceCollector();
        player.statusGUI.UpdateEssenceText();
        gameManager.SaveGame();
    }
    private void Withdraw()
    {
        player.essence += player.storedEssence;
        player.storedEssence = 0;
        UpdateEssenceCollector();
        player.statusGUI.UpdateEssenceText();
        gameManager.SaveGame();
    }
}
