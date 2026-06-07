using System;
using UnityEngine;
using EchoesOfArcadia.Core;

namespace EchoesOfArcadia.Data
{
    public enum PersonalStat
    {
        Insight,     // 洞察
        Courage,     // 勇気
        Empathy,     // 共感
        Expression,  // 表現
        Endurance    // 忍耐
    }

    public enum StatRank
    {
        Rank1,  // 最低
        Rank2,
        Rank3,
        Rank4,
        Rank5   // 最高
    }

    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        private static readonly string[] InsightRankNames = { "鈍感", "注意深い", "鋭敏", "透視", "真理の目" };
        private static readonly string[] CourageRankNames = { "臆病", "普通", "勇敢", "豪胆", "不屈" };
        private static readonly string[] EmpathyRankNames = { "無関心", "思いやり", "共感", "慈愛", "共鳴" };
        private static readonly string[] ExpressionRankNames = { "口下手", "素直", "雄弁", "詩人", "魂の声" };
        private static readonly string[] EnduranceRankNames = { "脆弱", "平均", "堅実", "鉄壁", "不動" };

        private static readonly int[] ThresholdsPerRank = { 0, 20, 50, 90, 140 };

        [SerializeField] private int[] statPoints = new int[5];

        public event Action<PersonalStat, StatRank> OnStatRankUp;

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

        public void AddPoints(PersonalStat stat, int points)
        {
            int index = (int)stat;
            var oldRank = GetRank(stat);
            statPoints[index] += points;
            var newRank = GetRank(stat);

            if (newRank > oldRank)
            {
                OnStatRankUp?.Invoke(stat, newRank);
                GameEventBus.Publish(new StatRankUpEvent(stat, newRank));
            }
        }

        public int GetPoints(PersonalStat stat) => statPoints[(int)stat];

        public StatRank GetRank(PersonalStat stat)
        {
            int pts = statPoints[(int)stat];
            for (int i = ThresholdsPerRank.Length - 1; i >= 0; i--)
            {
                if (pts >= ThresholdsPerRank[i])
                    return (StatRank)i;
            }
            return StatRank.Rank1;
        }

        public string GetRankName(PersonalStat stat)
        {
            int rank = (int)GetRank(stat);
            return stat switch
            {
                PersonalStat.Insight => InsightRankNames[rank],
                PersonalStat.Courage => CourageRankNames[rank],
                PersonalStat.Empathy => EmpathyRankNames[rank],
                PersonalStat.Expression => ExpressionRankNames[rank],
                PersonalStat.Endurance => EnduranceRankNames[rank],
                _ => ""
            };
        }

        public bool MeetsRequirement(PersonalStat stat, StatRank requiredRank)
        {
            return GetRank(stat) >= requiredRank;
        }
    }

    public readonly struct StatRankUpEvent
    {
        public readonly PersonalStat Stat;
        public readonly StatRank NewRank;

        public StatRankUpEvent(PersonalStat stat, StatRank newRank)
        {
            Stat = stat;
            NewRank = newRank;
        }
    }
}
