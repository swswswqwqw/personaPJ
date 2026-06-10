using System;
using System.IO;
using UnityEngine;

namespace AriaOfBacklight.Core
{
    [Serializable]
    public class SaveData
    {
        public int year;
        public int month;
        public int day;
        public int timeSlot;
        public int[] characterStats = new int[5];
        public int[] bondRanks = new int[8];
        public string currentScene;
        public float playTimeSeconds;
    }

    public static class SaveLoadManager
    {
        private const string SaveFolder = "Saves";
        private const int MaxSlots = 10;

        public static void Save(SaveData data, int slot)
        {
            var dir = Path.Combine(Application.persistentDataPath, SaveFolder);
            Directory.CreateDirectory(dir);
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetSavePath(slot), json);
        }

        public static SaveData Load(int slot)
        {
            var path = GetSavePath(slot);
            if (!File.Exists(path))
                return null;
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static bool SlotExists(int slot) =>
            File.Exists(GetSavePath(slot));

        private static string GetSavePath(int slot) =>
            Path.Combine(Application.persistentDataPath, SaveFolder, $"save_{slot:D2}.json");
    }
}
