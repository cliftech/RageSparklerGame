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
            return Directory.GetFiles(savePath, "*.sav").Length;
        } }

    public static void SaveProfile(SaveProfile profile)
    {
        string path = savePath + profile.id.ToString() + ".sav";

        try
        {
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
            Debug.LogWarning("Profile id exeeded profile count: " + id);
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
    public List<int> itemsInInventory;
    public List<int> itemsInHubChest;

    public SaveProfile(int id, int lvl, int essence, float timePlayed, List<int> itemsInInventory, List<int> itemsInHubChest)
    {
        this.id = id;
        this.lvl = lvl;
        this.essence = essence;
        this.timePlayed = timePlayed;
        this.itemsInInventory = itemsInInventory;
        this.itemsInHubChest = itemsInHubChest;
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
