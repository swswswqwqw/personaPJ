using System;
using UnityEngine;
using EchoesOfArcadia.Core;

namespace EchoesOfArcadia.Data
{
    public enum SocialStat
    {
        Courage,
        Intellect,
        Sensitivity,
        Charisma,
        Endurance
    }

    [Serializable]
    public class PlayerStatus
    {
        [Header("レベル・経験値")]
        public int level = 1;
        public int totalExp;

        [Header("内面ステータス（5段階・各100ポイントで次段階）")]
        public int courage;
        public int intellect;
        public int sensitivity;
        public int charisma;
        public int endurance;

        [Header("所持金")]
        public int money;

        public int GetStatRank(SocialStat stat)
        {
            int value = stat switch
            {
                SocialStat.Courage => courage,
                SocialStat.Intellect => intellect,
                SocialStat.Sensitivity => sensitivity,
                SocialStat.Charisma => charisma,
                SocialStat.Endurance => endurance,
                _ => 0
            };
            return value switch
            {
                < 30 => 1,
                < 70 => 2,
                < 130 => 3,
                < 200 => 4,
                _ => 5
            };
        }

        public string GetStatRankName(SocialStat stat)
        {
            int rank = GetStatRank(stat);
            return stat switch
            {
                SocialStat.Courage => rank switch
                {
                    1 => "臆病",
                    2 => "普通",
                    3 => "勇敢",
                    4 => "豪胆",
                    5 => "獅子心",
                    _ => ""
                },
                SocialStat.Intellect => rank switch
                {
                    1 => "無知",
                    2 => "平凡",
                    3 => "聡明",
                    4 => "博識",
                    5 => "叡智",
                    _ => ""
                },
                SocialStat.Sensitivity => rank switch
                {
                    1 => "鈍感",
                    2 => "平均",
                    3 => "敏感",
                    4 => "繊細",
                    5 => "共感者",
                    _ => ""
                },
                SocialStat.Charisma => rank switch
                {
                    1 => "地味",
                    2 => "普通",
                    3 => "魅力的",
                    4 => "華やか",
                    5 => "カリスマ",
                    _ => ""
                },
                SocialStat.Endurance => rank switch
                {
                    1 => "脆弱",
                    2 => "普通",
                    3 => "堅実",
                    4 => "不屈",
                    5 => "鉄心",
                    _ => ""
                },
                _ => ""
            };
        }

        public void AddStatPoints(SocialStat stat, int points)
        {
            switch (stat)
            {
                case SocialStat.Courage: courage += points; break;
                case SocialStat.Intellect: intellect += points; break;
                case SocialStat.Sensitivity: sensitivity += points; break;
                case SocialStat.Charisma: charisma += points; break;
                case SocialStat.Endurance: endurance += points; break;
            }

            GameEventBus.Publish(new SocialStatChangedEvent(stat, points, GetStatRank(stat)));
        }

        public bool MeetsRequirement(SocialStat stat, int requiredRank)
        {
            return GetStatRank(stat) >= requiredRank;
        }
    }

    public readonly struct SocialStatChangedEvent
    {
        public readonly SocialStat Stat;
        public readonly int PointsAdded;
        public readonly int NewRank;

        public SocialStatChangedEvent(SocialStat stat, int points, int rank)
        {
            Stat = stat;
            PointsAdded = points;
            NewRank = rank;
        }
    }
}
