using System;
using UnityEngine;
using ArcadiaOfEchoes.Core;

namespace ArcadiaOfEchoes.Core
{
    public enum InnerFrequency
    {
        Empathy,    // 共感力
        Courage,    // 胆力
        Intellect,  // 知性
        Expression, // 表現力
        Resolve     // 意志力
    }

    [Serializable]
    public class PlayerStatsData
    {
        public int Empathy;
        public int Courage;
        public int Intellect;
        public int Expression;
        public int Resolve;

        public const int MaxLevel = 5;
        public static readonly string[] RankNames = { "—", "萌芽", "成長", "開花", "覚醒", "共鳴" };
        public static readonly int[] ThresholdPerLevel = { 0, 15, 35, 60, 100 };
    }

    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        private PlayerStatsData _data = new();

        public int Empathy => _data.Empathy;
        public int Courage => _data.Courage;
        public int Intellect => _data.Intellect;
        public int Expression => _data.Expression;
        public int Resolve => _data.Resolve;

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

        public void AddStat(InnerFrequency stat, int amount)
        {
            int previous = GetStatValue(stat);
            int previousLevel = GetLevel(previous);

            switch (stat)
            {
                case InnerFrequency.Empathy: _data.Empathy += amount; break;
                case InnerFrequency.Courage: _data.Courage += amount; break;
                case InnerFrequency.Intellect: _data.Intellect += amount; break;
                case InnerFrequency.Expression: _data.Expression += amount; break;
                case InnerFrequency.Resolve: _data.Resolve += amount; break;
            }

            int newValue = GetStatValue(stat);
            int newLevel = GetLevel(newValue);

            if (newLevel > previousLevel)
                EventBus.Publish(new StatLevelUpEvent(stat, newLevel));
        }

        public int GetStatValue(InnerFrequency stat) => stat switch
        {
            InnerFrequency.Empathy => _data.Empathy,
            InnerFrequency.Courage => _data.Courage,
            InnerFrequency.Intellect => _data.Intellect,
            InnerFrequency.Expression => _data.Expression,
            InnerFrequency.Resolve => _data.Resolve,
            _ => 0
        };

        public int GetLevel(int value)
        {
            for (int i = PlayerStatsData.ThresholdPerLevel.Length - 1; i >= 0; i--)
            {
                if (value >= PlayerStatsData.ThresholdPerLevel[i])
                    return i + 1;
            }
            return 0;
        }

        public int GetStatLevel(InnerFrequency stat) => GetLevel(GetStatValue(stat));

        public string GetRankName(InnerFrequency stat)
        {
            int level = GetStatLevel(stat);
            return level < PlayerStatsData.RankNames.Length ? PlayerStatsData.RankNames[level] : "共鳴";
        }

        public bool MeetsRequirement(InnerFrequency stat, int requiredLevel)
        {
            return GetStatLevel(stat) >= requiredLevel;
        }

        public PlayerStatsData GetSaveData() => _data;
        public void LoadSaveData(PlayerStatsData data) => _data = data;
    }

    public readonly struct StatLevelUpEvent
    {
        public readonly InnerFrequency Stat;
        public readonly int NewLevel;
        public StatLevelUpEvent(InnerFrequency stat, int level) { Stat = stat; NewLevel = level; }
    }
}
