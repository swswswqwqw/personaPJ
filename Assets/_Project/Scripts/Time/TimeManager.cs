using UnityEngine;
using ArcanaOfHollows.Core;

namespace ArcanaOfHollows.Time
{
    public enum TimePeriod
    {
        Morning,
        Class,
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

        public int Year { get; private set; } = 2026;
        public int Month { get; private set; } = 4;
        public int Day { get; private set; } = 8;
        public TimePeriod CurrentPeriod { get; private set; } = TimePeriod.Morning;

        public Season CurrentSeason => Month switch
        {
            >= 3 and <= 5 => Season.Spring,
            >= 6 and <= 8 => Season.Summer,
            >= 9 and <= 11 => Season.Autumn,
            _ => Season.Winter
        };

        public string DayOfWeek => GetDayOfWeek();
        public string DateDisplay => $"{Month}月{Day}日（{DayOfWeek}）";
        public bool IsHoliday => calendarData?.IsHoliday(Month, Day) ?? false;
        public bool IsWeekend => DayOfWeek == "土" || DayOfWeek == "日";
        public bool HasFreeAction => CurrentPeriod == TimePeriod.AfterSchool || CurrentPeriod == TimePeriod.Evening;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AdvancePeriod()
        {
            var previous = CurrentPeriod;

            if (CurrentPeriod == TimePeriod.LateNight)
            {
                AdvanceDay();
                CurrentPeriod = TimePeriod.Morning;
            }
            else
            {
                CurrentPeriod++;
            }

            EventBus.Publish(new TimePeriodChangedEvent(previous, CurrentPeriod));
        }

        public void AdvanceToNextFreeTime()
        {
            while (!HasFreeAction)
            {
                AdvancePeriod();
            }
        }

        private void AdvanceDay()
        {
            var previousMonth = Month;
            Day++;

            int daysInMonth = GetDaysInMonth(Month);
            if (Day > daysInMonth)
            {
                Day = 1;
                Month++;
                if (Month > 12)
                {
                    Month = 1;
                    Year++;
                }
            }

            EventBus.Publish(new DayChangedEvent(Month, Day, DayOfWeek));

            if (Month != previousMonth)
                EventBus.Publish(new MonthChangedEvent(previousMonth, Month));
        }

        public int GetRemainingDays(int targetMonth, int targetDay)
        {
            int remaining = 0;
            int m = Month, d = Day;

            while (m < targetMonth || (m == targetMonth && d < targetDay))
            {
                d++;
                if (d > GetDaysInMonth(m))
                {
                    d = 1;
                    m++;
                }
                remaining++;
            }
            return remaining;
        }

        private string GetDayOfWeek()
        {
            var date = new System.DateTime(Year, Month, Day);
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

        private static int GetDaysInMonth(int month)
        {
            return month switch
            {
                2 => 28,
                4 or 6 or 9 or 11 => 30,
                _ => 31
            };
        }
    }

    public readonly struct TimePeriodChangedEvent
    {
        public readonly TimePeriod Previous;
        public readonly TimePeriod Current;

        public TimePeriodChangedEvent(TimePeriod previous, TimePeriod current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public readonly struct DayChangedEvent
    {
        public readonly int Month;
        public readonly int Day;
        public readonly string DayOfWeek;

        public DayChangedEvent(int month, int day, string dayOfWeek)
        {
            Month = month;
            Day = day;
            DayOfWeek = dayOfWeek;
        }
    }

    public readonly struct MonthChangedEvent
    {
        public readonly int PreviousMonth;
        public readonly int NewMonth;

        public MonthChangedEvent(int previousMonth, int newMonth)
        {
            PreviousMonth = previousMonth;
            NewMonth = newMonth;
        }
    }
}
