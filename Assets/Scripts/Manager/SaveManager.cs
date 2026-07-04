using System;
using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string path = Path.Combine(Application.persistentDataPath, "save.json");
    
    public static string Save(SaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);

        return "SAVED_SUCCESSFULLY";
    }
    
    public static bool HasSave() => File.Exists(path);

    public static SaveData Load()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            return data;
        }
        else
        {
            Debug.Log("SAVE_NOT_FOUND");
            return null;
        }
    }
}