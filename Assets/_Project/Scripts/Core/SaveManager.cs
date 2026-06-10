using System;
using System.IO;
using UnityEngine;
using AriaOfEchoes.Time;

namespace AriaOfEchoes.Core
{
    [Serializable]
    public class SaveData
    {
        public string saveId;
        public DateTime realTimestamp;

        // 時間
        public int year;
        public int month;
        public int day;
        public int timePeriod;
        public int weather;

        // プレイヤーステータス
        public int listening;
        public int voice;
        public int insight;
        public int guts;
        public int empathy;

        // プレイ時間
        public float totalPlayTimeSeconds;

        public GameDate ToGameDate() => new(year, month, day);
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        const int MAX_SAVE_SLOTS = 16;
        const string SAVE_DIR = "Saves";
        const string SAVE_PREFIX = "save_";

        float sessionStartTime;
        float accumulatedPlayTime;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            sessionStartTime = UnityEngine.Time.realtimeSinceStartup;
            EnsureSaveDirectory();
        }

        public void Save(int slot)
        {
            if (slot < 0 || slot >= MAX_SAVE_SLOTS) return;

            var data = BuildSaveData(slot);
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath(slot);
            File.WriteAllText(path, json);

            EventBus.Publish(new GameSavedEvent(slot));
        }

        public SaveData Load(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            EventBus.Publish(new GameLoadedEvent(slot, data));
            return data;
        }

        public bool SaveExists(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        public SaveData PeekSave(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
                File.Delete(path);
        }

        SaveData BuildSaveData(int slot)
        {
            var data = new SaveData
            {
                saveId = $"{SAVE_PREFIX}{slot}",
                realTimestamp = DateTime.Now,
                totalPlayTimeSeconds = accumulatedPlayTime
                    + (UnityEngine.Time.realtimeSinceStartup - sessionStartTime)
            };

            if (TimeManager.Instance != null)
            {
                var date = TimeManager.Instance.CurrentDate;
                data.year = date.Year;
                data.month = date.Month;
                data.day = date.Day;
                data.timePeriod = (int)TimeManager.Instance.CurrentPeriod;
                data.weather = (int)TimeManager.Instance.CurrentWeather;
            }

            if (Social.PlayerStats.Instance != null)
            {
                var ps = Social.PlayerStats.Instance;
                data.listening = ps.GetValue(Social.SocialStat.Listening);
                data.voice = ps.GetValue(Social.SocialStat.Voice);
                data.insight = ps.GetValue(Social.SocialStat.Insight);
                data.guts = ps.GetValue(Social.SocialStat.Guts);
                data.empathy = ps.GetValue(Social.SocialStat.Empathy);
            }

            return data;
        }

        string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, SAVE_DIR, $"{SAVE_PREFIX}{slot}.json");
        }

        void EnsureSaveDirectory()
        {
            string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }

    public struct GameSavedEvent
    {
        public int Slot;
        public GameSavedEvent(int slot) { Slot = slot; }
    }

    public struct GameLoadedEvent
    {
        public int Slot;
        public SaveData Data;
        public GameLoadedEvent(int slot, SaveData data) { Slot = slot; Data = data; }
    }
}
