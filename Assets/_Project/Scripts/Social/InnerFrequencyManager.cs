using System;
using System.Collections.Generic;
using AstralEchoes.Core;
using AstralEchoes.Data;

namespace AstralEchoes.Social
{
    public sealed class InnerFrequencyManager
    {
        static InnerFrequencyManager _instance;
        public static InnerFrequencyManager Instance => _instance ??= new InnerFrequencyManager();

        public const int MaxRank = 5;
        static readonly int[] RankThresholds = { 0, 12, 30, 54, 82, 120 };

        readonly Dictionary<InnerFrequency, int> _points = new();

        InnerFrequencyManager()
        {
            foreach (InnerFrequency stat in Enum.GetValues(typeof(InnerFrequency)))
                _points[stat] = 0;
        }

        public void AddPoints(InnerFrequency stat, int amount)
        {
            int previousRank = GetRank(stat);
            _points[stat] += amount;
            int newRank = GetRank(stat);

            if (newRank != previousRank)
            {
                EventBus.Publish(new InnerFrequencyChangedEvent
                {
                    Stat = stat,
                    PreviousValue = previousRank,
                    NewValue = newRank
                });
            }
        }

        public int GetPoints(InnerFrequency stat)
        {
            return _points.TryGetValue(stat, out int val) ? val : 0;
        }

        public int GetRank(InnerFrequency stat)
        {
            int points = GetPoints(stat);
            for (int i = MaxRank; i >= 0; i--)
            {
                if (points >= RankThresholds[i]) return i;
            }
            return 0;
        }

        public bool MeetsRequirement(InnerFrequency stat, int requiredRank)
        {
            return GetRank(stat) >= requiredRank;
        }

        public string GetRankName(InnerFrequency stat)
        {
            int rank = GetRank(stat);
            return stat switch
            {
                InnerFrequency.Empathy => rank switch
                {
                    0 => "無関心",
                    1 => "傾聴",
                    2 => "理解者",
                    3 => "共鳴者",
                    4 => "心の鏡",
                    5 => "深淵の共感",
                    _ => ""
                },
                InnerFrequency.Resolve => rank switch
                {
                    0 => "臆病",
                    1 => "少しの勇気",
                    2 => "毅然",
                    3 => "不退転",
                    4 => "鋼の意志",
                    5 => "絶対決意",
                    _ => ""
                },
                InnerFrequency.Insight => rank switch
                {
                    0 => "無知",
                    1 => "好奇心",
                    2 => "博識",
                    3 => "慧眼",
                    4 => "叡智",
                    5 => "万象洞察",
                    _ => ""
                },
                InnerFrequency.Allure => rank switch
                {
                    0 => "地味",
                    1 => "気になる存在",
                    2 => "注目株",
                    3 => "魅惑的",
                    4 => "カリスマ",
                    5 => "絶対魅了",
                    _ => ""
                },
                InnerFrequency.Harmony => rank switch
                {
                    0 => "不安定",
                    1 => "落ち着き",
                    2 => "調律",
                    3 => "静寂",
                    4 => "悟り",
                    5 => "完全調和",
                    _ => ""
                },
                _ => ""
            };
        }
    }
}
