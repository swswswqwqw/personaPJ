using System;
using System.Collections.Generic;
using AstralEchoes.Data;

namespace AstralEchoes.Time
{
    [Serializable]
    public class CalendarData
    {
        public int Year = 2026;
        public int StartMonth = 4;
        public int EndMonth = 3;

        public List<CalendarEvent> FixedEvents = new();
        public List<int> FullMoonDays = new() { 30 };
        public List<ExamPeriod> Exams = new();

        static readonly int[] DaysInMonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public static int GetDaysInMonth(int month)
        {
            if (month < 1 || month > 12) return 30;
            return DaysInMonth[month];
        }

        public static Season GetSeason(int month)
        {
            return month switch
            {
                >= 3 and <= 5 => Season.Spring,
                >= 6 and <= 8 => Season.Summer,
                >= 9 and <= 11 => Season.Autumn,
                _ => Season.Winter
            };
        }

        public static string GetDayOfWeek(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            return date.DayOfWeek switch
            {
                System.DayOfWeek.Monday => "月",
                System.DayOfWeek.Tuesday => "火",
                System.DayOfWeek.Wednesday => "水",
                System.DayOfWeek.Thursday => "木",
                System.DayOfWeek.Friday => "金",
                System.DayOfWeek.Saturday => "土",
                System.DayOfWeek.Sunday => "日",
                _ => "?"
            };
        }
    }

    [Serializable]
    public class CalendarEvent
    {
        public string Id;
        public string Name;
        public int Month;
        public int Day;
        public string Description;
        public bool IsDeadline;
        public string PalaceTargetId;
    }

    [Serializable]
    public class ExamPeriod
    {
        public string Id;
        public int Month;
        public int StartDay;
        public int EndDay;
        public int ResultDay;
    }
}
