using UnityEngine;
using ArcanaOfHollows.Core;

namespace ArcanaOfHollows.Player
{
    public enum SocialStat
    {
        Insight,    // 洞察
        Nerve,      // 胆力
        Empathy,    // 共感
        Eloquence,  // 話術
        Patience    // 忍耐
    }

    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        private const int MaxStatLevel = 5;
        private static readonly int[] PointsPerLevel = { 0, 6, 16, 30, 50, 80 };
        private static readonly string[] StatLevelNames =
        {
            "☆",
            "☆☆",
            "☆☆☆",
            "☆☆☆☆",
            "☆☆☆☆☆"
        };

        private int[] statPoints = new int[5];

        public int Level { get; private set; } = 1;
        public int Exp { get; private set; }
        public int Gold { get; private set; }

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

        public int GetStatLevel(SocialStat stat)
        {
            int points = statPoints[(int)stat];
            for (int level = MaxStatLevel; level >= 1; level--)
            {
                if (points >= PointsPerLevel[level])
                    return level;
            }
            return 0;
        }

        public int GetStatPoints(SocialStat stat) => statPoints[(int)stat];

        public string GetStatLevelName(SocialStat stat)
        {
            int level = GetStatLevel(stat);
            return level > 0 ? StatLevelNames[level - 1] : "—";
        }

        public string GetStatDisplayName(SocialStat stat)
        {
            return stat switch
            {
                SocialStat.Insight => "洞察",
                SocialStat.Nerve => "胆力",
                SocialStat.Empathy => "共感",
                SocialStat.Eloquence => "話術",
                SocialStat.Patience => "忍耐",
                _ => "?"
            };
        }

        public void AddStatPoints(SocialStat stat, int points)
        {
            int previousLevel = GetStatLevel(stat);
            statPoints[(int)stat] += points;

            int newLevel = GetStatLevel(stat);
            if (newLevel > previousLevel)
            {
                EventBus.Publish(new SocialStatLevelUpEvent(stat, newLevel));
            }

            EventBus.Publish(new SocialStatChangedEvent(stat, statPoints[(int)stat], newLevel));
        }

        public bool MeetsRequirement(SocialStat stat, int requiredLevel)
        {
            return GetStatLevel(stat) >= requiredLevel;
        }

        public void AddExp(int amount)
        {
            Exp += amount;
            int requiredExp = GetRequiredExp(Level);
            while (Exp >= requiredExp && Level < 99)
            {
                Exp -= requiredExp;
                Level++;
                requiredExp = GetRequiredExp(Level);
                EventBus.Publish(new PlayerLevelUpEvent(Level));
            }
        }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        private int GetRequiredExp(int level) => level * level * 10 + 50;
    }

    public readonly struct SocialStatChangedEvent
    {
        public readonly SocialStat Stat;
        public readonly int TotalPoints;
        public readonly int Level;

        public SocialStatChangedEvent(SocialStat stat, int totalPoints, int level)
        {
            Stat = stat;
            TotalPoints = totalPoints;
            Level = level;
        }
    }

    public readonly struct SocialStatLevelUpEvent
    {
        public readonly SocialStat Stat;
        public readonly int NewLevel;

        public SocialStatLevelUpEvent(SocialStat stat, int newLevel)
        {
            Stat = stat;
            NewLevel = newLevel;
        }
    }

    public readonly struct PlayerLevelUpEvent
    {
        public readonly int NewLevel;
        public PlayerLevelUpEvent(int newLevel) { NewLevel = newLevel; }
    }
}
