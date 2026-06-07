using System;

namespace Amane.Social
{
    /// <summary>
    /// 絆「言伝（ことづて）」1人分の進行状態。最大ランク10。
    /// ランク10で「最後の言伝」専用イベントが解放される。
    /// </summary>
    public sealed class Bond
    {
        public const int MaxRank = 10;
        // ランクnへ上がるのに必要な、そのランク帯での累積ポイント。
        private const int PointsPerRank = 100;

        public string Id { get; }
        public string DisplayName { get; }
        public Arcana Arcana { get; }
        public int Rank { get; private set; }
        public int PointsInRank { get; private set; }

        /// <summary>ランク10到達済みか（最後の言伝の解放条件）。</summary>
        public bool IsMaxed => Rank >= MaxRank;

        public Bond(string id, string displayName, Arcana arcana)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName;
            Arcana = arcana;
            Rank = 0;
            PointsInRank = 0;
        }

        /// <summary>
        /// 絆ポイントを加算する。同アルカナの語り手所持時は呼び出し側で x1.5 済みの値を渡す。
        /// ランクが上がったら true。
        /// </summary>
        public bool AddPoints(int amount)
        {
            if (amount <= 0 || IsMaxed) return false;
            bool rankedUp = false;
            PointsInRank += amount;
            while (PointsInRank >= PointsPerRank && Rank < MaxRank)
            {
                PointsInRank -= PointsPerRank;
                Rank++;
                rankedUp = true;
            }
            if (IsMaxed) PointsInRank = 0;
            return rankedUp;
        }
    }
}
