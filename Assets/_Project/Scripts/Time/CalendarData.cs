using System;
using System.Collections.Generic;
using UnityEngine;

namespace AriaOfBacklight.Time
{
    [CreateAssetMenu(fileName = "CalendarData", menuName = "AriaOfBacklight/CalendarData")]
    public class CalendarData : ScriptableObject
    {
        [SerializeField] private List<CalendarEvent> events = new();

        public CalendarEvent GetEvent(int month, int day)
        {
            return events.Find(e => e.month == month && e.day == day);
        }

        public List<CalendarEvent> GetMonthEvents(int month)
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

    [Serializable]
    public class CalendarEvent
    {
        public int month;
        public int day;
        public string eventName;
        [TextArea] public string description;
        public CalendarEventType eventType;
        public bool isDeadline;
        public string relatedRimenId;
    }

    public enum CalendarEventType
    {
        Story,
        Exam,
        Festival,
        Holiday,
        RimenDeadline,
        Special
    }
}
