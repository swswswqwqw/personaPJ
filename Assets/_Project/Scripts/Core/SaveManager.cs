using System;
using System.IO;
using UnityEngine;

namespace ArcadiaOfEchoes.Core
{
    [Serializable]
    public class SaveData
    {
        public string SaveDate;
        public Time.GameDate GameDate;
        public Time.TimePeriod TimePeriod;
        public PlayerStatsData PlayerStats;
        public string CurrentSceneName;
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const int MaxSaveSlots = 16;
        private const string SaveFilePrefix = "arcadia_save_";

        private void Awake()
        {
            if (Instance != null && Instance != this)
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

            var saveData = new SaveData
            {
                SaveDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                GameDate = Time.TimeManager.Instance?.CurrentDate ?? default,
                TimePeriod = Time.TimeManager.Instance?.CurrentPeriod ?? default,
                PlayerStats = PlayerStats.Instance?.GetSaveData(),
                CurrentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            };

            string json = JsonUtility.ToJson(saveData, true);
            string path = GetSavePath(slot);
            File.WriteAllText(path, json);

            EventBus.Publish(new GameSavedEvent(slot));
        }

        public SaveData Load(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots) return null;

            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public bool SaveExists(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"{SaveFilePrefix}{slot:D2}.json");
        }
    }

    public readonly struct GameSavedEvent
    {
        public readonly int Slot;
        public GameSavedEvent(int slot) { Slot = slot; }
    }
}
