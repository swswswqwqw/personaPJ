using System;
using System.IO;
using UnityEngine;
using EchoesOfArcadia.Data;
using EchoesOfArcadia.Time;

namespace EchoesOfArcadia.Core
{
    [Serializable]
    public class SaveData
    {
        public string saveId;
        public string timestamp;
        public GameDate gameDate;
        public TimePeriod timePeriod;
        public Weather weather;
        public PlayerStatus playerStatus;
        public string currentScene;
        public int playTimeSeconds;
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const int MaxSaveSlots = 16;
        private const string SavePrefix = "save_";
        private const string SaveExtension = ".json";

        private float _playTimeAccumulator;

        public int PlayTimeSeconds { get; private set; }

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

        private void Update()
        {
            _playTimeAccumulator += UnityEngine.Time.unscaledDeltaTime;
            if (_playTimeAccumulator >= 1f)
            {
                PlayTimeSeconds += (int)_playTimeAccumulator;
                _playTimeAccumulator %= 1f;
            }
        }

        public bool Save(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots) return false;

            var data = new SaveData
            {
                saveId = $"{SavePrefix}{slot}",
                timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                gameDate = TimeManager.Instance?.CurrentDate ?? new GameDate(2026, 4, 8),
                timePeriod = TimeManager.Instance?.CurrentPeriod ?? TimePeriod.Morning,
                weather = TimeManager.Instance?.CurrentWeather ?? Weather.Sunny,
                currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                playTimeSeconds = PlayTimeSeconds
            };

            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(slot);

            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(path, json);
                GameEventBus.Publish(new GameSavedEvent(slot));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
                return false;
            }
        }

        public SaveData Load(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<SaveData>(json);
                GameEventBus.Publish(new GameLoadedEvent(slot));
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return null;
            }
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

        public SaveData GetSaveInfo(int slot)
        {
            return Load(slot);
        }

        public string FormatPlayTime(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            return $"{hours:D2}:{minutes:D2}";
        }

        private string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, "saves", $"{SavePrefix}{slot}{SaveExtension}");
        }
    }

    public readonly struct GameSavedEvent
    {
        public readonly int Slot;
        public GameSavedEvent(int slot) => Slot = slot;
    }

    public readonly struct GameLoadedEvent
    {
        public readonly int Slot;
        public GameLoadedEvent(int slot) => Slot = slot;
    }
}
