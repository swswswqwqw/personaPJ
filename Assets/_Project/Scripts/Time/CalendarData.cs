using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadiaOfEchoes.Time
{
    [CreateAssetMenu(fileName = "CalendarData", menuName = "ArcadiaOfEchoes/Calendar Data")]
    public class CalendarData : ScriptableObject
    {
        [SerializeField] private List<CalendarEvent> events = new();
        [SerializeField] private List<DungeonDeadline> deadlines = new();
        [SerializeField] private List<ExamPeriod> exams = new();

        public IReadOnlyList<CalendarEvent> Events => events;
        public IReadOnlyList<DungeonDeadline> Deadlines => deadlines;
        public IReadOnlyList<ExamPeriod> Exams => exams;

        public List<CalendarEvent> GetEventsForDate(int month, int day)
        {
            return events.FindAll(e => e.Month == month && e.Day == day);
        }

        public DungeonDeadline GetActiveDeadline(int month, int day)
        {
            return deadlines.Find(d =>
                (month < d.DeadlineMonth || (month == d.DeadlineMonth && day <= d.DeadlineDay)) &&
                (month > d.StartMonth || (month == d.StartMonth && day >= d.StartDay)));
        }

        public bool IsExamPeriod(int month, int day)
        {
            return exams.Exists(e =>
                (month == e.StartMonth && day >= e.StartDay && day <= e.EndDay) ||
                (month == e.EndMonth && day >= e.StartDay && day <= e.EndDay));
        }
    }

    [Serializable]
    public class CalendarEvent
    {
        public string EventId;
        public string EventName;
        public int Month;
        public int Day;
        public CalendarEventType Type;
        public bool LocksTimeSlot;
        [TextArea] public string Description;
    }

    [Serializable]
    public class DungeonDeadline
    {
        public string DungeonId;
        public string DungeonName;
        public int StartMonth;
        public int StartDay;
        public int DeadlineMonth;
        public int DeadlineDay;
        public int WarningDaysBefore = 3;
    }

    [Serializable]
    public class ExamPeriod
    {
        public string ExamName;
        public int StartMonth;
        public int StartDay;
        public int EndMonth;
        public int EndDay;
    }

    public enum CalendarEventType
    {
        Story,
        Social,
        Holiday,
        SchoolEvent,
        Deadline,
        FreeEvent
    }
}
