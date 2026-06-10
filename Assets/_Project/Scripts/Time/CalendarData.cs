using System;
using System.Collections.Generic;
using UnityEngine;

namespace AriaOfEchoes.Time
{
    [CreateAssetMenu(fileName = "CalendarData", menuName = "AriaOfEchoes/CalendarData")]
    public class CalendarData : ScriptableObject
    {
        public List<CalendarEvent> events = new();
        public List<Deadline> deadlines = new();
        public List<ExamPeriod> examPeriods = new();

        public CalendarEvent GetEvent(GameDate date)
        {
            return events.Find(e => e.date == date);
        }

        public Deadline GetActiveDeadline(GameDate date)
        {
            return deadlines.Find(d => date >= d.startDate && date <= d.endDate);
        }

        public bool IsExamPeriod(GameDate date)
        {
            return examPeriods.Exists(e => date >= e.startDate && date <= e.endDate);
        }

        public bool IsHoliday(GameDate date)
        {
            var evt = GetEvent(date);
            return evt != null && evt.isHoliday;
        }
    }

    [Serializable]
    public class CalendarEvent
    {
        public string eventName;
        public GameDate date;
        public bool isHoliday;
        public bool restrictsAction;
        [TextArea] public string description;
    }

    [Serializable]
    public class Deadline
    {
        public string dungeonName;
        public GameDate startDate;
        public GameDate endDate;
        [TextArea] public string description;
    }

    [Serializable]
    public class ExamPeriod
    {
        public string examName;
        public GameDate startDate;
        public GameDate endDate;
    }
}
