using UnityEngine;
using System;

namespace Astra.Time
{
    [CreateAssetMenu(fileName = "CalendarData", menuName = "Astra/Time/CalendarData")]
    public class CalendarData : ScriptableObject
    {
        [Serializable]
        public struct SpecialDate
        {
            public int month;
            public int day;
            public string eventName;
            public bool isHoliday;
            public bool isDeadline;
        }

        [SerializeField] private SpecialDate[] specialDates;

        // Game starts on Monday, April 8th (2024 calendar as reference)
        private static readonly int StartDayOfWeekIndex = 0; // Monday

        private static readonly string[] DayNames = { "月", "火", "水", "木", "金", "土", "日" };

        private static readonly int[] DaysPerMonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public int GetDaysInMonth(int month)
        {
            if (month < 1 || month > 12) return 30;
            return DaysPerMonth[month];
        }

        public string GetDayOfWeek(int month, int day)
        {
            int totalDays = 0;
            for (int m = 4; m < month; m++)
                totalDays += DaysPerMonth[m];
            totalDays += (day - 8);

            int dayIndex = (StartDayOfWeekIndex + totalDays % 7 + 7) % 7;
            return DayNames[dayIndex];
        }

        public bool IsHoliday(int month, int day)
        {
            if (specialDates == null) return false;
            foreach (var date in specialDates)
            {
                if (date.month == month && date.day == day && date.isHoliday)
                    return true;
            }
            string dow = GetDayOfWeek(month, day);
            return dow == "土" || dow == "日";
        }

        public bool IsDeadline(int month, int day)
        {
            if (specialDates == null) return false;
            foreach (var date in specialDates)
            {
                if (date.month == month && date.day == day && date.isDeadline)
                    return true;
            }
            return false;
        }

        public string GetEventName(int month, int day)
        {
            if (specialDates == null) return null;
            foreach (var date in specialDates)
            {
                if (date.month == month && date.day == day)
                    return date.eventName;
            }
            return null;
        }
    }
}
