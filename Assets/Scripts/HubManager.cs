using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    [Header("Gameobjects to enable, when corresponding objects are unlocked")]
    public List<GameObject> portals;
    public GameObject smithNPC;
    public GameObject essenceCollector;

    public void LoadHub(HubSaveState hubSaveState)
    {
        for (int i = 0; i < portals.Count; i++)
        {
            portals[i].SetActive(hubSaveState.portalsUnlocked[i]);
        }
        smithNPC.SetActive(hubSaveState.smithUnlocked);
        essenceCollector.SetActive(hubSaveState.essenceCollectorUnlocked);
    }

    public HubSaveState DefaultSaveState()
    {
        HubSaveState hubSaveState = new HubSaveState();
        return hubSaveState;
    }
}

[System.Serializable]
public class HubSaveState
{
    public bool[] portalsUnlocked;
    public bool smithUnlocked;
    public bool essenceCollectorUnlocked;
    public int keeraState;
    public int essenceCollectorUpgrade;

    public HubSaveState()
    {
        portalsUnlocked = new bool[1000];
    }

    public void UnlockHubPortal(int index)
    {
        portalsUnlocked[index] = true;
    }
    public void UnlockHubSmithNpc()
    {
        smithUnlocked = true;
    }
    public void UnlockEssenceCollector()
    {
        essenceCollectorUnlocked = true;
    }
    public void SetKeeraState(int state)
    {
        keeraState = state;
    }
    public void SetEssenceCollectorUpgrade(int upgradeIndex)
    {
        essenceCollectorUpgrade = upgradeIndex;
    }
}
