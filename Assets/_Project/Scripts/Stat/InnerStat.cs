using System;
using System.Collections.Generic;

namespace Amane.Stat
{
    /// <summary>
    /// 主人公の内面5能力。DESIGN.md「ステータス成長」に対応。
    /// テーマ上、すべては上げきれない（時間の希少性）。
    /// </summary>
    public enum InnerStat
    {
        Courage,     // 度胸
        Intellect,   // 知性
        Empathy,     // 慈しみ
        Expression,  // ことのは
        Composure    // 静けさ
    }

    /// <summary>
    /// 内面ステータスの保持・成長を担う Plain C# クラス。
    /// 各ステータスは内部ポイントを持ち、閾値で 0..5 のランクへ写像する。
    /// </summary>
    public sealed class InnerStatSet
    {
        public const int MaxRank = 5;
        // ランクn達成に必要な累積ポイント（index = rank）。
        private static readonly int[] RankThresholds = { 0, 10, 30, 60, 100, 150 };

        private readonly Dictionary<InnerStat, int> _points = new();

        public InnerStatSet()
        {
            foreach (InnerStat s in Enum.GetValues(typeof(InnerStat)))
                _points[s] = 0;
        }

        public int GetPoints(InnerStat stat) => _points[stat];

        public int GetRank(InnerStat stat)
        {
            int p = _points[stat];
            int rank = 0;
            for (int r = 1; r <= MaxRank; r++)
                if (p >= RankThresholds[r]) rank = r;
            return rank;
        }

        /// <summary>ポイントを加算し、ランクが上がったら true を返す。</summary>
        public bool Add(InnerStat stat, int amount)
        {
            if (amount <= 0) return false;
            int before = GetRank(stat);
            _points[stat] = Math.Min(_points[stat] + amount, RankThresholds[MaxRank]);
            return GetRank(stat) > before;
        }

        /// <summary>指定ランク以上か（行動・絆・分岐の解放判定）。</summary>
        public bool Meets(InnerStat stat, int requiredRank) => GetRank(stat) >= requiredRank;
    }
}
