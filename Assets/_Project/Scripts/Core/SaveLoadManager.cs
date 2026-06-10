using UnityEngine;
using System;
using System.IO;

namespace Astra.Core
{
    [Serializable]
    public class SaveData
    {
        public int month;
        public int day;
        public int timePeriod;
        public int totalDaysElapsed;

        public int playerHP;
        public int playerSP;
        public int listening;
        public int courage;
        public int intellect;
        public int empathy;
        public int expression;

        public int[] resonanceLinkRanks;
        public int[] resonanceLinkPoints;

        public string currentScene;
        public float playTimeSeconds;
    }

    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        public const int MaxSaveSlots = 10;

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
            var data = CreateSaveData();
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(slot);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, json);

            EventBus.Publish(new GameSavedEvent(slot));
        }

        public SaveData Load(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);

            EventBus.Publish(new GameLoadedEvent(slot));
            return data;
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

        private SaveData CreateSaveData()
        {
            var data = new SaveData();

            var time = Time.TimeManager.Instance;
            if (time != null)
            {
                data.month = time.CurrentMonth;
                data.day = time.CurrentDay;
                data.timePeriod = (int)time.CurrentPeriod;
                data.totalDaysElapsed = time.TotalDaysElapsed;
            }

            var stats = PlayerStats.Instance;
            if (stats != null)
            {
                data.listening = stats.Listening;
                data.courage = stats.Courage;
                data.intellect = stats.Intellect;
                data.empathy = stats.Empathy;
                data.expression = stats.Expression;
            }

            return data;
        }

        private string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, "saves", $"save_{slot:D2}.json");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public readonly struct GameSavedEvent
    {
        public readonly int Slot;
        public GameSavedEvent(int slot) { Slot = slot; }
    }

    public readonly struct GameLoadedEvent
    {
        public readonly int Slot;
        public GameLoadedEvent(int slot) { Slot = slot; }
    }
}
