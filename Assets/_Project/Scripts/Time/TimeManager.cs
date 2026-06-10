using UnityEngine;
using AriaOfBacklight.Core;

namespace AriaOfBacklight.Time
{
    public enum TimeSlot
    {
        Morning,
        Class,
        Lunch,
        AfterSchool,
        Evening,
        LateNight
    }

    public enum Weather
    {
        Sunny,
        Cloudy,
        Rainy,
        Storm
    }

    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [SerializeField] private CalendarData calendarData;

        public int Year { get; private set; } = 2025;
        public int Month { get; private set; } = 4;
        public int Day { get; private set; } = 8;
        public TimeSlot CurrentTimeSlot { get; private set; } = TimeSlot.Morning;
        public Weather CurrentWeather { get; private set; } = Weather.Sunny;

        public int RemainingActions { get; private set; }

        private static readonly int[] ActionsPerSlot = { 0, 1, 1, 2, 1, 1 };

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RemainingActions = ActionsPerSlot[(int)CurrentTimeSlot];
        }

        public void ConsumeAction()
        {
            if (RemainingActions <= 0) return;

            RemainingActions--;
            if (RemainingActions <= 0)
                AdvanceTimeSlot();
        }

        public void AdvanceTimeSlot()
        {
            var previousSlot = CurrentTimeSlot;

            if (CurrentTimeSlot == TimeSlot.LateNight)
            {
                AdvanceDay();
                CurrentTimeSlot = TimeSlot.Morning;
            }
            else
            {
                CurrentTimeSlot++;
            }

            RemainingActions = ActionsPerSlot[(int)CurrentTimeSlot];
            EventBus.Publish(new TimeAdvancedEvent(previousSlot, CurrentTimeSlot, Month, Day));
        }

        public void AdvanceDay()
        {
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
                EventBus.Publish(new MonthChangedEvent(Month));
            }

            CurrentWeather = GenerateWeather();
            EventBus.Publish(new DayChangedEvent(Year, Month, Day, CurrentWeather));
        }

        public string GetDateString() => $"{Month}月{Day}日（{GetDayOfWeekJP()}）";

        public string GetTimeSlotName() => CurrentTimeSlot switch
        {
            TimeSlot.Morning => "朝",
            TimeSlot.Class => "授業",
            TimeSlot.Lunch => "昼休み",
            TimeSlot.AfterSchool => "放課後",
            TimeSlot.Evening => "夜",
            TimeSlot.LateNight => "深夜",
            _ => ""
        };

        public bool IsSchoolDay()
        {
            int dow = GetDayOfWeek();
            return dow != 0 && dow != 6;
        }

        private int GetDayOfWeek()
        {
            var date = new System.DateTime(Year, Month, Day);
            return (int)date.DayOfWeek;
        }

        private string GetDayOfWeekJP()
        {
            string[] names = { "日", "月", "火", "水", "木", "金", "土" };
            return names[GetDayOfWeek()];
        }

        private int GetDaysInMonth(int month) => month switch
        {
            2 => System.DateTime.IsLeapYear(Year) ? 29 : 28,
            4 or 6 or 9 or 11 => 30,
            _ => 31
        };

        private Weather GenerateWeather()
        {
            float roll = Random.value;
            return Month switch
            {
                6 or 7 => roll < 0.4f ? Weather.Rainy : roll < 0.6f ? Weather.Cloudy : Weather.Sunny,
                12 or 1 or 2 => roll < 0.3f ? Weather.Cloudy : Weather.Sunny,
                _ => roll < 0.15f ? Weather.Rainy : roll < 0.35f ? Weather.Cloudy : Weather.Sunny
            };
        }
    }

    public readonly struct TimeAdvancedEvent
    {
        public readonly TimeSlot PreviousSlot;
        public readonly TimeSlot CurrentSlot;
        public readonly int Month;
        public readonly int Day;

        public TimeAdvancedEvent(TimeSlot previous, TimeSlot current, int month, int day)
        {
            PreviousSlot = previous;
            CurrentSlot = current;
            Month = month;
            Day = day;
        }
    }

    public readonly struct DayChangedEvent
    {
        public readonly int Year;
        public readonly int Month;
        public readonly int Day;
        public readonly Weather Weather;

        public DayChangedEvent(int year, int month, int day, Weather weather)
        {
            Year = year;
            Month = month;
            Day = day;
            Weather = weather;
        }
    }

    public readonly struct MonthChangedEvent
    {
        public readonly int Month;
        public MonthChangedEvent(int month) { Month = month; }
    }
}
