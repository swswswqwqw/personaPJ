using System;
using AstralEchoes.Core;
using AstralEchoes.Data;

namespace AstralEchoes.Time
{
    public sealed class TimeManager
    {
        static TimeManager _instance;
        public static TimeManager Instance => _instance ??= new TimeManager();

        public int CurrentMonth { get; private set; } = 4;
        public int CurrentDay { get; private set; } = 1;
        public TimeOfDay CurrentTimeOfDay { get; private set; } = TimeOfDay.Morning;
        public float ActionPointsRemaining { get; private set; }

        readonly CalendarData _calendar = new();

        TimeManager()
        {
            RefreshActionPoints();
        }

        public void Initialize(int month, int day, TimeOfDay time)
        {
            CurrentMonth = month;
            CurrentDay = day;
            CurrentTimeOfDay = time;
            RefreshActionPoints();
        }

        public bool CanAct() => ActionPointsRemaining > 0f;

        public bool TrySpendActionPoints(float cost)
        {
            if (ActionPointsRemaining < cost) return false;
            ActionPointsRemaining -= cost;
            EventBus.Publish(new ActionPointsChangedEvent
            {
                Remaining = ActionPointsRemaining,
                Used = cost
            });
            return true;
        }

        public void AdvanceTime()
        {
            var previous = CurrentTimeOfDay;

            CurrentTimeOfDay = CurrentTimeOfDay switch
            {
                TimeOfDay.Morning => TimeOfDay.Class,
                TimeOfDay.Class => TimeOfDay.AfterSchool,
                TimeOfDay.AfterSchool => TimeOfDay.Evening,
                TimeOfDay.Evening => TimeOfDay.LateNight,
                TimeOfDay.LateNight => TimeOfDay.Morning,
                _ => TimeOfDay.Morning
            };

            if (CurrentTimeOfDay == TimeOfDay.Morning)
            {
                AdvanceDay();
            }

            RefreshActionPoints();

            EventBus.Publish(new TimeAdvancedEvent
            {
                PreviousTime = previous,
                NewTime = CurrentTimeOfDay,
                Day = CurrentDay,
                Month = CurrentMonth
            });
        }

        void AdvanceDay()
        {
            var previousDay = CurrentDay;
            CurrentDay++;

            int daysInMonth = CalendarData.GetDaysInMonth(CurrentMonth);
            if (CurrentDay > daysInMonth)
            {
                CurrentDay = 1;
                CurrentMonth++;
                if (CurrentMonth > 12) CurrentMonth = 1;
            }

            EventBus.Publish(new DayAdvancedEvent
            {
                PreviousDay = previousDay,
                NewDay = CurrentDay,
                Month = CurrentMonth
            });
        }

        void RefreshActionPoints()
        {
            ActionPointsRemaining = CurrentTimeOfDay switch
            {
                TimeOfDay.AfterSchool => 1.0f,
                TimeOfDay.Evening => 1.0f,
                TimeOfDay.LateNight => 0.5f,
                _ => 0f
            };
        }

        public bool IsFullMoon() => _calendar.FullMoonDays.Contains(CurrentDay);

        public bool IsWeekend()
        {
            var dayOfWeek = CalendarData.GetDayOfWeek(_calendar.Year, CurrentMonth, CurrentDay);
            return dayOfWeek is "土" or "日";
        }

        public Season GetCurrentSeason() => CalendarData.GetSeason(CurrentMonth);

        public string GetDateString() =>
            $"{CurrentMonth}月{CurrentDay}日({CalendarData.GetDayOfWeek(_calendar.Year, CurrentMonth, CurrentDay)})";

        public string GetTimeOfDayString() => CurrentTimeOfDay switch
        {
            TimeOfDay.Morning => "朝",
            TimeOfDay.Class => "授業",
            TimeOfDay.AfterSchool => "放課後",
            TimeOfDay.Evening => "夜",
            TimeOfDay.LateNight => "深夜",
            _ => ""
        };
    }
}
