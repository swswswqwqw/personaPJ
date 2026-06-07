using System;
using System.IO;
using UnityEngine;

namespace EchoesOfArcadia.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const int MaxSaveSlots = 16;
        private const string SaveFilePrefix = "save_";
        private const string SaveFileExtension = ".json";

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Save(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots) return;

            var saveData = CollectSaveData();
            string json = JsonUtility.ToJson(saveData, true);
            string path = GetSavePath(slotIndex);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
                GameEventBus.Publish(new SaveCompletedEvent(slotIndex, true));
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
                GameEventBus.Publish(new SaveCompletedEvent(slotIndex, false));
            }
        }

        public SaveData Load(int slotIndex)
        {
            string path = GetSavePath(slotIndex);
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return null;
            }
        }

        public bool SaveExists(int slotIndex)
        {
            return File.Exists(GetSavePath(slotIndex));
        }

        public void DeleteSave(int slotIndex)
        {
            string path = GetSavePath(slotIndex);
            if (File.Exists(path))
                File.Delete(path);
        }

        private SaveData CollectSaveData()
        {
            var data = new SaveData
            {
                saveId = Guid.NewGuid().ToString(),
                realWorldSaveTime = DateTime.Now
            };

            if (TimeSystem.TimeManager.Instance != null)
            {
                data.currentDate = TimeSystem.TimeManager.Instance.CurrentDate;
                data.currentTimeOfDay = TimeSystem.TimeManager.Instance.CurrentTimeOfDay;
            }

            if (Data.PlayerStats.Instance != null)
            {
                for (int i = 0; i < 5; i++)
                    data.personalStats[i] = Data.PlayerStats.Instance.GetPoints((Data.PersonalStat)i);
            }

            return data;
        }

        private void ApplySaveData(SaveData data)
        {
            // TODO: 各マネージャーにセーブデータを復元する
        }

        private string GetSavePath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, "Saves",
                $"{SaveFilePrefix}{slotIndex:D2}{SaveFileExtension}");
        }
    }

    public readonly struct SaveCompletedEvent
    {
        public readonly int SlotIndex;
        public readonly bool Success;

        public SaveCompletedEvent(int slotIndex, bool success)
        {
            SlotIndex = slotIndex;
            Success = success;
        }
    }
}
