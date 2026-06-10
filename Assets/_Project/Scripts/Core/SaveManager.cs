using System;
using System.IO;
using UnityEngine;
using ArcanaOfHollows.Player;
using ArcanaOfHollows.Time;

namespace ArcanaOfHollows.Core
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

        public void Save(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots) return;

            var data = CreateSaveData();
            var json = JsonUtility.ToJson(data, true);
            var path = GetSavePath(slot);

            try
            {
                File.WriteAllText(path, json);
                EventBus.Publish(new SaveCompleteEvent(slot, true));
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
                EventBus.Publish(new SaveCompleteEvent(slot, false));
            }
        }

        public bool Load(int slot)
        {
            var path = GetSavePath(slot);
            if (!File.Exists(path)) return false;

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                ApplySaveData(data);
                EventBus.Publish(new LoadCompleteEvent(slot, true));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                EventBus.Publish(new LoadCompleteEvent(slot, false));
                return false;
            }
        }

        public bool SaveExists(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        public SaveSlotInfo GetSlotInfo(int slot)
        {
            var path = GetSavePath(slot);
            if (!File.Exists(path))
                return new SaveSlotInfo { isEmpty = true };

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                return new SaveSlotInfo
                {
                    isEmpty = false,
                    dateDisplay = $"{data.month}月{data.day}日",
                    timePeriod = ((TimePeriod)data.timePeriod).ToString(),
                    playerLevel = data.playerLevel,
                    playTime = data.playTimeSeconds,
                    savedAt = data.savedAt
                };
            }
            catch
            {
                return new SaveSlotInfo { isEmpty = true };
            }
        }

        private SaveData CreateSaveData()
        {
            var timeManager = TimeManager.Instance;
            var playerStats = PlayerStats.Instance;

            return new SaveData
            {
                version = 1,
                savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                year = timeManager?.Year ?? 2026,
                month = timeManager?.Month ?? 4,
                day = timeManager?.Day ?? 8,
                timePeriod = (int)(timeManager?.CurrentPeriod ?? TimePeriod.Morning),
                playerLevel = playerStats?.Level ?? 1,
                playerExp = playerStats?.Exp ?? 0,
                playerGold = playerStats?.Gold ?? 0,
                playTimeSeconds = UnityEngine.Time.realtimeSinceStartup
            };
        }

        private void ApplySaveData(SaveData data)
        {
            // TimeManager and PlayerStats will read from this data
            // Full implementation requires each manager to have a LoadState method
            Debug.Log($"Loaded save: {data.month}/{data.day} Lv.{data.playerLevel}");
        }

        private string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"{SaveFilePrefix}{slot:D2}{SaveFileExtension}");
        }
    }

    [Serializable]
    public class SaveData
    {
        public int version;
        public string savedAt;
        public int year;
        public int month;
        public int day;
        public int timePeriod;
        public int playerLevel;
        public int playerExp;
        public int playerGold;
        public float playTimeSeconds;
        public int[] socialStatPoints = new int[5];
        public HeartStringSaveData[] heartStrings;
        public string[] storyFlags;
    }

    [Serializable]
    public class HeartStringSaveData
    {
        public string characterId;
        public int rank;
        public float points;
        public bool isUnlocked;
    }

    public struct SaveSlotInfo
    {
        public bool isEmpty;
        public string dateDisplay;
        public string timePeriod;
        public int playerLevel;
        public float playTime;
        public string savedAt;
    }

    public readonly struct SaveCompleteEvent
    {
        public readonly int Slot;
        public readonly bool Success;
        public SaveCompleteEvent(int slot, bool success) { Slot = slot; Success = success; }
    }

    public readonly struct LoadCompleteEvent
    {
        public readonly int Slot;
        public readonly bool Success;
        public LoadCompleteEvent(int slot, bool success) { Slot = slot; Success = success; }
    }
}
