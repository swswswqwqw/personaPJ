using System;
using System.IO;
using UnityEngine;

namespace Amane.Core
{
    public sealed class JsonSaveSystem : ISaveSystem
    {
        private const string SaveDir = "Saves";
        private readonly Func<SaveData> _serialize;
        private readonly Action<SaveData> _deserialize;

        public JsonSaveSystem(Func<SaveData> serialize, Action<SaveData> deserialize)
        {
            _serialize = serialize;
            _deserialize = deserialize;
        }

        public bool HasSave(int slot) => File.Exists(SlotPath(slot));

        public void Save(int slot)
        {
            var data = _serialize();
            data.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data, true);
            Directory.CreateDirectory(SaveDirPath());
            File.WriteAllText(SlotPath(slot), json);
            Debug.Log($"[Save] Slot {slot} saved.");
        }

        public bool Load(int slot)
        {
            string path = SlotPath(slot);
            if (!File.Exists(path)) return false;
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            _deserialize(data);
            Debug.Log($"[Save] Slot {slot} loaded.");
            return true;
        }

        private static string SaveDirPath() => Path.Combine(Application.persistentDataPath, SaveDir);
        private static string SlotPath(int slot) => Path.Combine(SaveDirPath(), $"save_{slot}.json");
    }
}
