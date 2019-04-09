using System.Collections;
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

    private void Start()
    {
        collectorGUI = FindObjectOfType<EssenceCollectorGUI>();
        player = FindObjectOfType<Player>();
        fill = transform.GetChild(0);
        maxEssence = 10;
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
        player.storedEssence += player.essence;
        player.essence = 0;
        UpdateEssenceCollector();
        player.statusGUI.UpdateEssenceText();
    }
    private void Withdraw()
    {
        player.essence += player.storedEssence;
        player.storedEssence = 0;
        UpdateEssenceCollector();
        player.statusGUI.UpdateEssenceText();
    }
}
