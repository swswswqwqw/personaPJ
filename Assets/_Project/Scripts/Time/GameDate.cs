using System;

namespace Amane.Time
{
    /// <summary>
    /// ゲーム内カレンダーの日付。4月開始〜翌3月終了の学年暦。
    /// 通日（DayIndex, 4/1 = 1）で内部管理し、月日へ変換する。
    /// うるう年は扱わず、固定の日数テーブルで簡潔に保つ。
    /// </summary>
    public readonly struct GameDate : IEquatable<GameDate>
    {
        // 学年暦の並び: 4,5,6,7,8,9,10,11,12,1,2,3
        private static readonly int[] MonthOrder = { 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3 };
        private static readonly int[] DaysInMonth = { 30, 31, 30, 31, 31, 30, 31, 30, 31, 31, 28, 31 };

        /// <summary>4/1 を 1 とする通日。</summary>
        public int DayIndex { get; }

        public GameDate(int dayIndex)
        {
            if (dayIndex < 1) throw new ArgumentOutOfRangeException(nameof(dayIndex));
            DayIndex = dayIndex;
        }

        /// <summary>学年の総日数。</summary>
        public static int TotalDays
        {
            get
            {
                int sum = 0;
                foreach (var d in DaysInMonth) sum += d;
                return sum;
            }
        }

        public int Month
        {
            get
            {
                int remaining = DayIndex;
                for (int i = 0; i < DaysInMonth.Length; i++)
                {
                    if (remaining <= DaysInMonth[i]) return MonthOrder[i];
                    remaining -= DaysInMonth[i];
                }
                return MonthOrder[MonthOrder.Length - 1];
            }
        }

        public int Day
        {
            get
            {
                int remaining = DayIndex;
                for (int i = 0; i < DaysInMonth.Length; i++)
                {
                    if (remaining <= DaysInMonth[i]) return remaining;
                    remaining -= DaysInMonth[i];
                }
                return DaysInMonth[DaysInMonth.Length - 1];
            }
        }

        /// <summary>日曜=0 … 土曜=6。4/1 を月曜(1)始まりと仮定する。</summary>
        public int DayOfWeek => (DayIndex) % 7; // 0..6

        public GameDate AddDays(int days) => new GameDate(DayIndex + days);

        public bool IsLastDay => DayIndex >= TotalDays;

        public override string ToString() => $"{Month}月{Day}日";

        public bool Equals(GameDate other) => DayIndex == other.DayIndex;
        public override bool Equals(object obj) => obj is GameDate d && Equals(d);
        public override int GetHashCode() => DayIndex;
        public static bool operator ==(GameDate a, GameDate b) => a.Equals(b);
        public static bool operator !=(GameDate a, GameDate b) => !a.Equals(b);
    }
}
