using System;

namespace AriaOfEchoes.Time
{
    [Serializable]
    public struct GameDate : IEquatable<GameDate>, IComparable<GameDate>
    {
        public int Year;
        public int Month;
        public int Day;

        public GameDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                var dt = new DateTime(Year, Month, Day);
                return dt.DayOfWeek;
            }
        }

        public bool IsWeekend => DayOfWeek == System.DayOfWeek.Saturday
                              || DayOfWeek == System.DayOfWeek.Sunday;

        public string DayOfWeekJP => DayOfWeek switch
        {
            System.DayOfWeek.Sunday => "日",
            System.DayOfWeek.Monday => "月",
            System.DayOfWeek.Tuesday => "火",
            System.DayOfWeek.Wednesday => "水",
            System.DayOfWeek.Thursday => "木",
            System.DayOfWeek.Friday => "金",
            System.DayOfWeek.Saturday => "土",
            _ => "?"
        };

        public string SeasonJP
        {
            get
            {
                return Month switch
                {
                    3 or 4 or 5 => "春",
                    6 or 7 or 8 => "夏",
                    9 or 10 or 11 => "秋",
                    12 or 1 or 2 => "冬",
                    _ => ""
                };
            }
        }

        public GameDate NextDay()
        {
            var dt = new DateTime(Year, Month, Day).AddDays(1);
            return new GameDate(dt.Year, dt.Month, dt.Day);
        }

        public int DaysUntil(GameDate target)
        {
            var current = new DateTime(Year, Month, Day);
            var targetDt = new DateTime(target.Year, target.Month, target.Day);
            return (int)(targetDt - current).TotalDays;
        }

        public override string ToString() => $"{Month}月{Day}日（{DayOfWeekJP}）";

        public string ToFullString() => $"{Year}年{Month}月{Day}日（{DayOfWeekJP}）";

        public bool Equals(GameDate other)
            => Year == other.Year && Month == other.Month && Day == other.Day;

        public override bool Equals(object obj)
            => obj is GameDate other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Year, Month, Day);

        public int CompareTo(GameDate other)
        {
            int c = Year.CompareTo(other.Year);
            if (c != 0) return c;
            c = Month.CompareTo(other.Month);
            if (c != 0) return c;
            return Day.CompareTo(other.Day);
        }

        public static bool operator ==(GameDate a, GameDate b) => a.Equals(b);
        public static bool operator !=(GameDate a, GameDate b) => !a.Equals(b);
        public static bool operator <(GameDate a, GameDate b) => a.CompareTo(b) < 0;
        public static bool operator >(GameDate a, GameDate b) => a.CompareTo(b) > 0;
        public static bool operator <=(GameDate a, GameDate b) => a.CompareTo(b) <= 0;
        public static bool operator >=(GameDate a, GameDate b) => a.CompareTo(b) >= 0;
    }
}
