using UnityEngine;
using System;
using System.IO;
using JetBrains.Annotations;

[Serializable]
public class LocalFileScoreRepository : IScoreRepository
{
    static string FilePath => Path.Combine(Application.persistentDataPath, "HighScore.json");
    [NotNull]
    public SavedScoreInformation Load()
    {
        if (!File.Exists(FilePath))
        {
            return new();
        }

        string json = File.ReadAllText(FilePath);
    
        if (string.IsNullOrWhiteSpace(json))
        {
            return new();
        }
    
        SavedScoreInformation score = JsonUtility.FromJson<SavedScoreInformation>(json);
        return score ?? new();
    }

    public void Save(SavedScoreInformation score)
    {
        string json = JsonUtility.ToJson(score, prettyPrint: true);
        string directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(FilePath, json);
    }
}