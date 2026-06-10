using System;
using System.IO;
using UnityEngine;

namespace ArcadiaOfEchoes.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const int MaxSaveSlots = 16;
        private const string SaveFilePrefix = "arcadia_save_";

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

        public void SaveGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSaveSlots) return;

            var data = CollectSaveData();
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(slotIndex);

            try
            {
                File.WriteAllText(path, json);
                Debug.Log($"Game saved to slot {slotIndex}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save: {e.Message}");
            }
        }

        public bool LoadGame(int slotIndex)
        {
            string path = GetSavePath(slotIndex);
            if (!File.Exists(path)) return false;

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                ApplySaveData(data);
                Debug.Log($"Game loaded from slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load: {e.Message}");
                return false;
            }
        }

        public bool SaveExists(int slotIndex)
        {
            return File.Exists(GetSavePath(slotIndex));
        }

        public SaveSlotInfo GetSlotInfo(int slotIndex)
        {
            string path = GetSavePath(slotIndex);
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                return new SaveSlotInfo
                {
                    SlotIndex = slotIndex,
                    SaveDate = data.SaveTimestamp,
                    GameMonth = data.CurrentMonth,
                    GameDay = data.CurrentDay,
                    PlayTimeSeconds = data.PlayTimeSeconds
                };
            }
            catch
            {
                return null;
            }
        }

        private string GetSavePath(int slotIndex)
        {
            return Path.Combine(Application.persistentDataPath, $"{SaveFilePrefix}{slotIndex}.json");
        }

        private SaveData CollectSaveData()
        {
            var timeManager = Time.TimeManager.Instance;
            var playerStats = Core.PlayerStats.Instance;

            return new SaveData
            {
                SaveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                CurrentMonth = timeManager?.CurrentMonth ?? 4,
                CurrentDay = timeManager?.CurrentDay ?? 8,
                CurrentTimeSlot = (int)(timeManager?.CurrentTimeSlot ?? 0),
                TotalDaysElapsed = timeManager?.TotalDaysElapsed ?? 0,
                PlayTimeSeconds = UnityEngine.Time.realtimeSinceStartup,
                CouragePoints = playerStats?.GetStatValue(Data.SocialStatType.Courage) ?? 0,
                WisdomPoints = playerStats?.GetStatValue(Data.SocialStatType.Wisdom) ?? 0,
                SensibilityPoints = playerStats?.GetStatValue(Data.SocialStatType.Sensibility) ?? 0,
                GutsPoints = playerStats?.GetStatValue(Data.SocialStatType.Guts) ?? 0,
                EmpathyPoints = playerStats?.GetStatValue(Data.SocialStatType.Empathy) ?? 0
            };
        }

        private void ApplySaveData(SaveData data)
        {
            // TODO: Apply loaded data to game systems
            Debug.Log($"Applying save data: Month {data.CurrentMonth}, Day {data.CurrentDay}");
        }
    }

    [Serializable]
    public class SaveData
    {
        public string SaveTimestamp;
        public int CurrentMonth;
        public int CurrentDay;
        public int CurrentTimeSlot;
        public int TotalDaysElapsed;
        public float PlayTimeSeconds;
        public int CouragePoints;
        public int WisdomPoints;
        public int SensibilityPoints;
        public int GutsPoints;
        public int EmpathyPoints;
        // TODO: Add resonance ranks, inventory, story flags
    }

    public class SaveSlotInfo
    {
        public int SlotIndex;
        public string SaveDate;
        public int GameMonth;
        public int GameDay;
        public float PlayTimeSeconds;
    }
}
