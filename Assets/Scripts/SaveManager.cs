using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager
{
    public static string savePath = (Application.isEditor ?
        Application.dataPath                       // use this only in editor mode
                                        :
        Application.persistentDataPath)            // use this in built verion of the game
        + "/SaveData/";

    public static int profileCount { get
        {
            if (!Directory.Exists(savePath))
                return 0;
            return Directory.GetFiles(savePath, "*.sav").Length;
        } }

    public static void SaveProfile(SaveProfile profile)
    {
        string path = savePath + profile.id.ToString() + ".sav";

        try
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            if (File.Exists(path))
            {
                Debug.Log("Overriding: \"" + path + "\"");
                File.Delete(path);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, profile);
            stream.Close();

            Debug.Log("Saved to: \"" + path + "\"");
        }
        catch (IOException e)
        {
            Debug.LogWarning("Saving Failed: \"" + path + "\"\n " + e.Message);
        }
    }

    public static SaveProfile LoadProfile(int id)
    {
        if (id >= profileCount)
        {
            Debug.LogWarning(string.Format("Profile id: {0} exeeded profile count: {1}", id, profileCount));
        }

        string path = savePath + id.ToString() + ".sav";

        try
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            SaveProfile profile = (SaveProfile)formatter.Deserialize(stream);
            stream.Close();

            Debug.Log("Loaded from: \"" + path + "\"");
            return profile;
        }
        catch (IOException e)
        {
            Debug.LogWarning("Loading Failed: \"" + path + "\"\n " + e.Message);
        }
        return null;
    }

}

[System.Serializable]
public class SaveProfile
{
    public int id;
    public int lvl;
    public int essence;
    public float timePlayed;
    public int numberOfDeaths;
    public List<string> itemsInInventory;
    public List<string> itemsInHubChest;
    public List<int> checkpoints;
    public float xPosInHub, yPosInHub;
    public bool hubUnloked;

    public bool dashUnlocked;
    public bool midAirDashUnlocked;
    public bool downwardAttackUnlocked;
    public bool wallJumpingUnlocked;
    public int maxJumpCount = 2;
    public float dashDistance = 3;
    public float minDelayBetweenDashes = 0.2f;
    public int maxMidairDashesCount = 1;
    public float invincibilityFrameTime = 0.5f;

    public SaveProfile(int id, int lvl, int essence, float timePlayed, int numberOfDeaths, 
        List<string> itemsInInventory, List<string> itemsInHubChest, List<int> checkpoints, float xPosInHub, float yPosInHub, bool hubUnloked,
        bool dashUnlocked, bool midAirDashUnlocked, bool downwardAttackUnlocked, bool wallJumpingUnlocked, int maxJumpCount, 
        float dashDistance, float minDelayBetweenDashes, int maxMidairDashesCount, float invincibilityFrameTime)
    {
        this.id = id;
        this.lvl = lvl;
        this.essence = essence;
        this.timePlayed = timePlayed;
        this.numberOfDeaths = numberOfDeaths;
        this.itemsInInventory = itemsInInventory;
        this.itemsInHubChest = itemsInHubChest;
        this.checkpoints = checkpoints;
        this.xPosInHub = xPosInHub;
        this.yPosInHub = yPosInHub;
        this.hubUnloked = hubUnloked;

        this.dashUnlocked = dashUnlocked;
        this.midAirDashUnlocked = midAirDashUnlocked;
        this.downwardAttackUnlocked = downwardAttackUnlocked;
        this.wallJumpingUnlocked = wallJumpingUnlocked;
        this.maxJumpCount = maxJumpCount;
        this.dashDistance = dashDistance;
        this.minDelayBetweenDashes = minDelayBetweenDashes;
        this.maxMidairDashesCount = maxMidairDashesCount;
        this.invincibilityFrameTime = invincibilityFrameTime;
    }

    public override string ToString()
    {
        string res = string.Format("id:{0}, lvl{1}, essence{2}, timePlayed:{3}\nItems in inventory:\n", id, lvl, essence, timePlayed);
        foreach (var s in itemsInInventory)
            res += s + ", ";
        res += "\nItems in hub chest:\n";
        foreach (var s in itemsInHubChest)
            res += s + ", ";
        return res;
    }
}
