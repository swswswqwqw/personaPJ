using System;
using System.Collections.Generic;
using AstralEchoes.Data;

namespace AstralEchoes.Save
{
    public sealed class SaveManager
    {
        static SaveManager _instance;
        public static SaveManager Instance => _instance ??= new SaveManager();

        public const int MaxSlots = 16;

        SaveManager() { }

        public SaveData CreateSaveData()
        {
            var time = Time.TimeManager.Instance;

            return new SaveData
            {
                SlotIndex = -1,
                Timestamp = DateTime.Now,
                PlayTimeSeconds = 0,
                Month = time.CurrentMonth,
                Day = time.CurrentDay,
                TimeOfDay = time.CurrentTimeOfDay,
                CurrentScene = Core.SceneTransitionManager.Instance.CurrentScene,
                // Other systems would serialize their state here
            };
        }

        public bool Save(int slotIndex, SaveData data)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots) return false;
            data.SlotIndex = slotIndex;
            data.Timestamp = DateTime.Now;

            string json = SerializeToJson(data);
            // In Unity: File.WriteAllText(GetSavePath(slotIndex), json);
            return true;
        }

        public SaveData Load(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots) return null;
            // In Unity: string json = File.ReadAllText(GetSavePath(slotIndex));
            // return DeserializeFromJson(json);
            return null;
        }

        public bool HasSaveData(int slotIndex)
        {
            // In Unity: return File.Exists(GetSavePath(slotIndex));
            return false;
        }

        public bool HasAnySaveData()
        {
            for (int i = 0; i < MaxSlots; i++)
                if (HasSaveData(i)) return true;
            return false;
        }

        string SerializeToJson(SaveData data)
        {
            // In Unity: return JsonUtility.ToJson(data, true);
            return "{}";
        }
    }

    [Serializable]
    public class SaveData
    {
        public int SlotIndex;
        public DateTime Timestamp;
        public double PlayTimeSeconds;

        public int Month;
        public int Day;
        public TimeOfDay TimeOfDay;
        public string CurrentScene;

        public Dictionary<string, int> AttunementRanks = new();
        public Dictionary<InnerFrequency, int> InnerFrequencyPoints = new();
        public List<string> OwnedEchoIds = new();
        public string EquippedEchoId;
        public int Money;

        public CharacterStats ProtagonistStats;
        public List<PartyMemberSaveData> PartyMembers = new();
    }

    [Serializable]
    public class PartyMemberSaveData
    {
        public string CharacterId;
        public CharacterStats Stats;
        public string EchoId;
    }
}
