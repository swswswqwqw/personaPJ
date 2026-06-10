using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfArcadia.Time
{
    public enum CalendarEventType
    {
        Story,
        Deadline,
        SchoolEvent,
        SeasonalEvent,
        Holiday,
        Exam,
        FreeDay
    }

    [Serializable]
    public class CalendarEvent
    {
        public string eventName;
        public CalendarEventType eventType;
        public GameDate date;
        [TextArea(1, 3)] public string description;
        public bool isManuallyTriggered;
        public string triggerScriptId;
    }

    [CreateAssetMenu(fileName = "CalendarData", menuName = "EchoesOfArcadia/Calendar Data")]
    public class CalendarData : ScriptableObject
    {
        public List<CalendarEvent> events = new();

        public List<CalendarEvent> GetEventsForDate(GameDate date)
        {
            var result = new List<CalendarEvent>();
            foreach (var e in events)
            {
                if (e.date.Equals(date))
                    result.Add(e);
            }
            return result;
        }

        public CalendarEvent GetNextDeadline(GameDate currentDate)
        {
            CalendarEvent nearest = null;
            int nearestDays = int.MaxValue;

            foreach (var e in events)
            {
                if (e.eventType != CalendarEventType.Deadline) continue;
                int days = currentDate.DaysUntil(e.date);
                if (days > 0 && days < nearestDays)
                {
                    nearest = e;
                    nearestDays = days;
                }
            }
            return nearest;
        }

        public int GetDaysUntilDeadline(GameDate currentDate)
        {
            var deadline = GetNextDeadline(currentDate);
            return deadline != null ? currentDate.DaysUntil(deadline.date) : -1;
        }
    }
}
