using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcanaOfHollows.Time
{
    [CreateAssetMenu(fileName = "CalendarData", menuName = "ArcanaOfHollows/Calendar Data")]
    public class CalendarData : ScriptableObject
    {
        [Serializable]
        public class CalendarEvent
        {
            public int month;
            public int day;
            public string eventName;
            public string description;
            public bool isHoliday;
            public bool isDeadline;
            public string triggerEventId;
        }

        [SerializeField] private List<CalendarEvent> events = new();

        public bool IsHoliday(int month, int day)
        {
            return events.Exists(e => e.month == month && e.day == day && e.isHoliday);
        }

        public CalendarEvent GetEvent(int month, int day)
        {
            return events.Find(e => e.month == month && e.day == day);
        }

        public List<CalendarEvent> GetEventsForMonth(int month)
        {
            return events.FindAll(e => e.month == month);
        }

        public CalendarEvent GetNextDeadline(int currentMonth, int currentDay)
        {
            return events.Find(e =>
                e.isDeadline &&
                (e.month > currentMonth || (e.month == currentMonth && e.day > currentDay)));
        }
    }
}
