using System;
using UnityEngine;
using AriaOfEchoes.Core;

namespace AriaOfEchoes.Social
{
    public enum SocialStat
    {
        Listening,  // 傾聴力
        Voice,      // 表現力
        Insight,    // 洞察力
        Guts,       // 胆力
        Empathy     // 共感力
    }

    [Serializable]
    public class SocialStatValues
    {
        public int listening;
        public int voice;
        public int insight;
        public int guts;
        public int empathy;

        public int Get(SocialStat stat)
        {
            return stat switch
            {
                SocialStat.Listening => listening,
                SocialStat.Voice => voice,
                SocialStat.Insight => insight,
                SocialStat.Guts => guts,
                SocialStat.Empathy => empathy,
                _ => 0
            };
        }

        public void Add(SocialStat stat, int amount)
        {
            switch (stat)
            {
                case SocialStat.Listening: listening += amount; break;
                case SocialStat.Voice: voice += amount; break;
                case SocialStat.Insight: insight += amount; break;
                case SocialStat.Guts: guts += amount; break;
                case SocialStat.Empathy: empathy += amount; break;
            }
        }
    }

    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [SerializeField] int[] rankThresholds = { 0, 20, 50, 100, 180, 300 };

        SocialStatValues statValues = new();

        public event Action<SocialStat, int, int> OnStatChanged;
        public event Action<SocialStat, int> OnStatRankUp;

        static readonly string[] ListeningRankNames =
            { "無関心", "聞き役", "相談相手", "傾聴者", "共鳴者" };
        static readonly string[] VoiceRankNames =
            { "無口", "つぶやき", "語り手", "弁舌家", "声の導き手" };
        static readonly string[] InsightRankNames =
            { "無知", "観察者", "洞察者", "看破者", "真理の目" };
        static readonly string[] GutsRankNames =
            { "臆病", "平常心", "度胸", "豪胆", "不屈の魂" };
        static readonly string[] EmpathyRankNames =
            { "無感覚", "察知", "共感", "慈愛", "全受容" };

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddStatPoints(SocialStat stat, int points)
        {
            int oldRank = GetRank(stat);
            int oldValue = statValues.Get(stat);

            statValues.Add(stat, points);

            int newValue = statValues.Get(stat);
            int newRank = GetRank(stat);

            OnStatChanged?.Invoke(stat, oldValue, newValue);
            EventBus.Publish(new SocialStatChangedEvent(stat, oldValue, newValue));

            if (newRank > oldRank)
            {
                OnStatRankUp?.Invoke(stat, newRank);
                EventBus.Publish(new SocialStatRankUpEvent(stat, newRank));
            }
        }

        public int GetValue(SocialStat stat) => statValues.Get(stat);

        public int GetRank(SocialStat stat)
        {
            int value = statValues.Get(stat);
            for (int i = rankThresholds.Length - 1; i >= 0; i--)
            {
                if (value >= rankThresholds[i])
                    return i + 1;
            }
            return 1;
        }

        public int GetMaxRank() => rankThresholds.Length;

        public string GetRankName(SocialStat stat)
        {
            int rank = GetRank(stat) - 1;
            return stat switch
            {
                SocialStat.Listening => ListeningRankNames[Mathf.Clamp(rank, 0, 4)],
                SocialStat.Voice => VoiceRankNames[Mathf.Clamp(rank, 0, 4)],
                SocialStat.Insight => InsightRankNames[Mathf.Clamp(rank, 0, 4)],
                SocialStat.Guts => GutsRankNames[Mathf.Clamp(rank, 0, 4)],
                SocialStat.Empathy => EmpathyRankNames[Mathf.Clamp(rank, 0, 4)],
                _ => ""
            };
        }

        public string GetStatDisplayName(SocialStat stat)
        {
            return stat switch
            {
                SocialStat.Listening => "傾聴力",
                SocialStat.Voice => "表現力",
                SocialStat.Insight => "洞察力",
                SocialStat.Guts => "胆力",
                SocialStat.Empathy => "共感力",
                _ => ""
            };
        }

        public bool MeetsRequirement(SocialStat stat, int requiredRank)
        {
            return GetRank(stat) >= requiredRank;
        }
    }

    public struct SocialStatChangedEvent
    {
        public SocialStat Stat;
        public int OldValue;
        public int NewValue;
        public SocialStatChangedEvent(SocialStat s, int o, int n)
        { Stat = s; OldValue = o; NewValue = n; }
    }

    public struct SocialStatRankUpEvent
    {
        public SocialStat Stat;
        public int NewRank;
        public SocialStatRankUpEvent(SocialStat s, int r)
        { Stat = s; NewRank = r; }
    }
}
