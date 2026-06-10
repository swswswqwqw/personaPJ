using System;
using UnityEngine;
using AriaOfBacklight.Core;

namespace AriaOfBacklight.Social
{
    public enum PlayerStatType
    {
        Intelligence, // 知性
        Courage,      // 度胸
        Sensitivity,  // 感性
        Dexterity,    // 器用さ
        Empathy       // 共感力
    }

    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        private static readonly string[] RankNames =
        {
            "未熟", "平凡", "堅実", "優秀", "達人"
        };

        private static readonly string[][] StatRankNames =
        {
            new[] { "ぼんやり", "平凡", "聡明", "博識", "叡智" },
            new[] { "臆病", "普通", "勇敢", "豪胆", "不屈" },
            new[] { "鈍感", "平凡", "繊細", "芸術肌", "共感覚" },
            new[] { "不器用", "並", "器用", "匠", "神業" },
            new[] { "無関心", "普通", "理解者", "慈愛", "調律師" }
        };

        [SerializeField] private int[] statExp = new int[5];

        private int[] statLevels = new int[5];

        private static readonly int[] ExpThresholds = { 0, 20, 55, 100, 170 };

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

        public int GetStat(PlayerStatType stat) => statLevels[(int)stat];

        public string GetStatRankName(PlayerStatType stat)
        {
            int level = statLevels[(int)stat];
            return StatRankNames[(int)stat][Mathf.Clamp(level, 0, 4)];
        }

        public void AddExp(PlayerStatType stat, int amount)
        {
            int index = (int)stat;
            statExp[index] += amount;

            int newLevel = 0;
            for (int i = ExpThresholds.Length - 1; i >= 0; i--)
            {
                if (statExp[index] >= ExpThresholds[i])
                {
                    newLevel = i;
                    break;
                }
            }

            if (newLevel > statLevels[index])
            {
                statLevels[index] = newLevel;
                EventBus.Publish(new StatLevelUpEvent(stat, newLevel, GetStatRankName(stat)));
            }
        }
    }

    public readonly struct StatLevelUpEvent
    {
        public readonly PlayerStatType Stat;
        public readonly int NewLevel;
        public readonly string RankName;

        public StatLevelUpEvent(PlayerStatType stat, int newLevel, string rankName)
        { Stat = stat; NewLevel = newLevel; RankName = rankName; }
    }
}
