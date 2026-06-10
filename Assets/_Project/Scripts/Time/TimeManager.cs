using UnityEngine;
using Astra.Core;

namespace Astra.Time
{
    public enum TimePeriod
    {
        Morning,
        Class,
        Lunch,
        AfterSchool,
        Evening,
        LateNight
    }

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [SerializeField] private CalendarData calendarData;

        public int CurrentMonth { get; private set; } = 4;
        public int CurrentDay { get; private set; } = 8;
        public TimePeriod CurrentPeriod { get; private set; } = TimePeriod.Morning;
        public int TotalDaysElapsed { get; private set; } = 0;

        public Season CurrentSeason => CurrentMonth switch
        {
            >= 3 and <= 5 => Season.Spring,
            >= 6 and <= 8 => Season.Summer,
            >= 9 and <= 11 => Season.Autumn,
            _ => Season.Winter
        };

        public string DayOfWeek => calendarData?.GetDayOfWeek(CurrentMonth, CurrentDay) ?? "月";

        public bool CanActInPeriod(TimePeriod period)
        {
            return period switch
            {
                TimePeriod.Lunch => true,
                TimePeriod.AfterSchool => true,
                TimePeriod.Evening => true,
                TimePeriod.LateNight => PlayerStats.Instance != null && PlayerStats.Instance.Courage >= 3,
                _ => false
            };
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AdvancePeriod()
        {
            var previousPeriod = CurrentPeriod;

            if (CurrentPeriod == TimePeriod.LateNight || CurrentPeriod == TimePeriod.Evening)
            {
                AdvanceDay();
                CurrentPeriod = TimePeriod.Morning;
            }
            else
            {
                CurrentPeriod = (TimePeriod)((int)CurrentPeriod + 1);
            }

            EventBus.Publish(new TimeAdvancedEvent(previousPeriod, CurrentPeriod, CurrentMonth, CurrentDay));
        }

        private void AdvanceDay()
        {
            TotalDaysElapsed++;
            CurrentDay++;

            int daysInMonth = calendarData?.GetDaysInMonth(CurrentMonth) ?? 30;
            if (CurrentDay > daysInMonth)
            {
                CurrentDay = 1;
                CurrentMonth++;
                if (CurrentMonth > 12) CurrentMonth = 1;

                EventBus.Publish(new MonthChangedEvent(CurrentMonth));
            }

            EventBus.Publish(new DayChangedEvent(CurrentMonth, CurrentDay, TotalDaysElapsed));
        }

        public int GetRemainingDaysInMonth()
        {
            int daysInMonth = calendarData?.GetDaysInMonth(CurrentMonth) ?? 30;
            return daysInMonth - CurrentDay;
        }
    }

    public readonly struct TimeAdvancedEvent
    {
        public readonly TimePeriod From;
        public readonly TimePeriod To;
        public readonly int Month;
        public readonly int Day;

        public TimeAdvancedEvent(TimePeriod from, TimePeriod to, int month, int day)
        {
            From = from;
            To = to;
            Month = month;
            Day = day;
        }
    }

    public readonly struct DayChangedEvent
    {
        public readonly int Month;
        public readonly int Day;
        public readonly int TotalDaysElapsed;

        public DayChangedEvent(int month, int day, int totalDaysElapsed)
        {
            Month = month;
            Day = day;
            TotalDaysElapsed = totalDaysElapsed;
        }
    }

    public readonly struct MonthChangedEvent
    {
        public readonly int NewMonth;

        public MonthChangedEvent(int newMonth)
        {
            NewMonth = newMonth;
        }
    }
}
