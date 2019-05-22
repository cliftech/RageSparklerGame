using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class SaveManager
{
    public static string savePath = (Application.isEditor ?
        Application.dataPath                       // use this only in editor mode
                                        :
        Application.persistentDataPath)            // use this in built verion of the game
        + "/SaveData/";

    public static string settingsDirPath = (Application.isEditor ?
        Application.dataPath                       // use this only in editor mode
                                        :
        Application.persistentDataPath)            // use this in built verion of the game
        + "/SaveData/";

    public static string settingsPath = settingsDirPath + "/settings.ini";

    public static int profileCount
    {
        get
        {
            if (!Directory.Exists(savePath))
                return 0;
            return Directory.GetFiles(savePath, "*.sav").Length;
        }
    }

    public static void ValidateSaves()
    {
        int count = profileCount;
        for (int i = 0; i < count; i++)
        {
            if (LoadProfile(i) == null)
            {
                DirectoryInfo dinfo = new DirectoryInfo(savePath);
                foreach (FileInfo finfo in dinfo.GetFiles())
                {
                    finfo.Delete();
                }
                return;
            }
        }
    }

    public static void SaveProfile(SaveProfile profile)
    {
        string path = savePath + profile.id.ToString() + ".sav";

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

        try
        {
            formatter.Serialize(stream, profile);
            stream.Close();

            Debug.Log("Saved to: \"" + path + "\"");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Saving Failed: \"" + path + "\"\n " + e.Message);
            stream.Close();
        }
    }

    public static SaveProfile LoadProfile(int id)
    {
        if (id >= profileCount)
        {
            Debug.LogWarning(string.Format("Profile id: {0} exeeded profile count: {1}", id, profileCount));
        }

        string path = savePath + id.ToString() + ".sav";

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        try
        {
            SaveProfile profile = (SaveProfile)formatter.Deserialize(stream);
            stream.Close();

            Debug.Log("Loaded from: \"" + path + "\"");
            return profile;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Loading Failed: \"" + path + "\"\n " + e.Message);
            stream.Close();
        }
        return null;
    }

    public static void DeleteProfile(int id)
    {
        string path = savePath + id.ToString() + ".sav";
        Debug.Log("Deleting: " + path);

        int pCount = profileCount;

        File.Delete(path);
        //Debug.LogWarning("deleting: " + path);

        bool rewrittenSaves = false;

        for (int i = id + 1; i < pCount; i++)
        {
            SaveProfile p = LoadProfile(i);
            p.id -= 1;
            //Debug.LogWarning("saving " + (p.id + 1) + " as " + p.id);
            SaveProfile(p);
            rewrittenSaves = true;
        }

        if (rewrittenSaves)
        {
            //Debug.LogWarning("deleting: " + savePath + (pCount - 1).ToString() + ".sav");
            File.Delete(savePath + (pCount - 1).ToString() + ".sav");
        }

        Settings settings = LoadSettings();
        settings.lastSavedProfile = -1;
        SaveSettings(settings);
    }

    public static void ExhangeProfiles(int id1, int id2)
    {
        Debug.Log("Exchanging profiles: " + id1 + " and " + id2);
        if (id1 == id2)
            return;
        SaveProfile p1 = LoadProfile(id1);
        SaveProfile p2 = LoadProfile(id2);

        int tmp = p1.id;
        p1.id = p2.id;
        p2.id = tmp;

        SaveProfile(p1);
        SaveProfile(p2);
    }

    public static void SaveSettings(Settings settings)
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
        if (!Directory.Exists(settingsDirPath))
            Directory.CreateDirectory(settingsDirPath);

        if (File.Exists(settingsPath))
        {
            Debug.Log("Overriding: \"" + settingsPath + "\"");
            File.Delete(settingsPath);
        }

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(settingsPath, FileMode.Create, FileAccess.Write);

        try
        {
            formatter.Serialize(stream, settings);
            stream.Close();

            Debug.Log("Saved to: \"" + settingsPath + "\"");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Saving Failed: \"" + settingsPath + "\"\n " + e.Message);
            stream.Close();
        }
    }

    public static Settings LoadSettings()
    {
        if (!File.Exists(settingsPath))
            return null;

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(settingsPath, FileMode.Open, FileAccess.Read);
        try
        {
            Settings settings = (Settings)formatter.Deserialize(stream);
            stream.Close();

            Debug.Log("Loaded from: \"" + settingsPath + "\"");
            return settings;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Loading Failed: \"" + settingsPath + "\"\n " + e.Message);
            stream.Close();
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
    public int essenceStored;
    public float timePlayed;
    public int numberOfDeaths;
    public List<string> itemsInInventory;
    public List<int> itemInInventoryAmounts;
    public List<string> itemsInHubChest;
    public List<int> itemInHubChestAmounts;
    public List<int> checkpoints;
    public Dictionary<string, int> enemyKillCount;
    public int lastHubPortalID;
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

    public HubSaveState hubSaveState;

    public SaveProfile(int id, int lvl, int essence, int essenceStored, float timePlayed, int numberOfDeaths,
        List<string> itemsInInventory, List<int> itemInInventoryAmounts, List<string> itemsInHubChest, List<int> itemInHubChestAmounts,
        Dictionary<string, int> enemyKillCount, List<int> checkpoints, int lastHubPortalID, bool hubUnloked,
        bool dashUnlocked, bool midAirDashUnlocked, bool downwardAttackUnlocked, bool wallJumpingUnlocked, int maxJumpCount,
        float dashDistance, float minDelayBetweenDashes, int maxMidairDashesCount, float invincibilityFrameTime,
        HubSaveState hubSaveState)
    {
        this.id = id;
        this.lvl = lvl;
        this.essence = essence;
        this.essenceStored = essenceStored;
        this.timePlayed = timePlayed;
        this.numberOfDeaths = numberOfDeaths;
        this.itemsInInventory = itemsInInventory;
        this.itemInInventoryAmounts = itemInInventoryAmounts;
        this.itemsInHubChest = itemsInHubChest;
        this.itemInHubChestAmounts = itemInHubChestAmounts;
        this.enemyKillCount = enemyKillCount;
        this.checkpoints = checkpoints;
        this.hubUnloked = hubUnloked;
        this.lastHubPortalID = lastHubPortalID;

        this.dashUnlocked = dashUnlocked;
        this.midAirDashUnlocked = midAirDashUnlocked;
        this.downwardAttackUnlocked = downwardAttackUnlocked;
        this.wallJumpingUnlocked = wallJumpingUnlocked;
        this.maxJumpCount = maxJumpCount;
        this.dashDistance = dashDistance;
        this.minDelayBetweenDashes = minDelayBetweenDashes;
        this.maxMidairDashesCount = maxMidairDashesCount;
        this.invincibilityFrameTime = invincibilityFrameTime;

        this.hubSaveState = hubSaveState;
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

[System.Serializable]
public class Settings
{
    public int lastSavedProfile;
    public int profileToLoad;
    public bool firstTimeLoadingProfile;
    public float masterVolumeSliderValue, soundfxVolumeSliderValue, musicVolumeSliderValue;

    public Settings(int lastSavedProfile, int profileToLoad, bool firstTimeLoadingProfile, float masterVolumeSliderValue, float soundfxVolumeSliderValue, float musicVolumeSliderValue)
    {
        this.lastSavedProfile = lastSavedProfile;
        this.profileToLoad = profileToLoad;
        this.firstTimeLoadingProfile = firstTimeLoadingProfile;
        this.masterVolumeSliderValue = masterVolumeSliderValue;
        this.soundfxVolumeSliderValue = soundfxVolumeSliderValue;
        this.musicVolumeSliderValue = musicVolumeSliderValue;
    }

    public override string ToString()
    {
        return masterVolumeSliderValue + " " + soundfxVolumeSliderValue + " " + musicVolumeSliderValue;
    }
}
