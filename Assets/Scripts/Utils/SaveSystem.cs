using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public bool hasRedKey;
}

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/savegame.json";
    
    public static void SaveGame(bool redKeyStatus)
    {
        SaveData data = new SaveData();
        data.hasRedKey = redKeyStatus;
        
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
    }
    
    public static SaveData LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }
    
    public static bool HasSave()
    {
        return File.Exists(savePath);
    }
    
    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    }
}