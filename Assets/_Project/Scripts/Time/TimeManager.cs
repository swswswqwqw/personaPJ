using UnityEngine;
using ArcadiaOfEchoes.Core;

namespace ArcadiaOfEchoes.Time
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [SerializeField] private int startMonth = 4;
        [SerializeField] private int startDay = 8;

        public int CurrentMonth { get; private set; }
        public int CurrentDay { get; private set; }
        public TimeSlot CurrentTimeSlot { get; private set; }
        public DayOfWeek CurrentDayOfWeek { get; private set; }
        public WeatherType CurrentWeather { get; private set; }

        public bool IsHoliday => CurrentDayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        public int TotalDaysElapsed { get; private set; }

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

        public void Initialize()
        {
            CurrentMonth = startMonth;
            CurrentDay = startDay;
            CurrentTimeSlot = TimeSlot.Morning;
            TotalDaysElapsed = 0;
            RecalculateDayOfWeek();
            RollWeather();
        }

        public void AdvanceTimeSlot()
        {
            var oldSlot = CurrentTimeSlot;
            CurrentTimeSlot = GetNextTimeSlot(CurrentTimeSlot);

            if (CurrentTimeSlot == TimeSlot.Morning)
            {
                AdvanceDay();
            }

            EventBus.Publish(new TimeSlotChangedEvent
            {
                OldSlot = oldSlot,
                NewSlot = CurrentTimeSlot
            });
        }

        public bool CanActInCurrentSlot()
        {
            return CurrentTimeSlot switch
            {
                TimeSlot.AfterSchool => true,
                TimeSlot.Evening => true,
                TimeSlot.LateNight => HasLateNightAccess(),
                _ => false
            };
        }

        public int GetActionPoints(TimeSlot slot)
        {
            return slot switch
            {
                TimeSlot.AfterSchool => 1,
                TimeSlot.Evening => 1,
                TimeSlot.LateNight => HasLateNightAccess() ? 1 : 0,
                _ => 0
            };
        }

        private void AdvanceDay()
        {
            CurrentDay++;
            TotalDaysElapsed++;

            int daysInMonth = GetDaysInMonth(CurrentMonth);
            if (CurrentDay > daysInMonth)
            {
                CurrentDay = 1;
                CurrentMonth++;
                if (CurrentMonth > 12)
                    CurrentMonth = 1;
            }

            RecalculateDayOfWeek();
            RollWeather();

            EventBus.Publish(new DayAdvancedEvent
            {
                NewDay = CurrentDay,
                NewMonth = CurrentMonth
            });
        }

        private TimeSlot GetNextTimeSlot(TimeSlot current)
        {
            return current switch
            {
                TimeSlot.Morning => TimeSlot.School,
                TimeSlot.School => TimeSlot.AfterSchool,
                TimeSlot.AfterSchool => TimeSlot.Evening,
                TimeSlot.Evening => TimeSlot.LateNight,
                TimeSlot.LateNight => TimeSlot.Morning,
                _ => TimeSlot.Morning
            };
        }

        private bool HasLateNightAccess()
        {
            // Requires Guts rank 3+
            // TODO: Connect to PlayerStats when implemented
            return false;
        }

        private void RecalculateDayOfWeek()
        {
            // 2025年4月8日は火曜日をベースに計算
            int baseDayOfWeek = 2; // Tuesday
            int daysFromBase = TotalDaysElapsed;
            int dayIndex = (baseDayOfWeek + daysFromBase) % 7;
            CurrentDayOfWeek = (DayOfWeek)dayIndex;
        }

        private void RollWeather()
        {
            float roll = Random.value;
            CurrentWeather = CurrentMonth switch
            {
                6 or 7 => roll < 0.4f ? WeatherType.Rainy : roll < 0.7f ? WeatherType.Cloudy : WeatherType.Sunny,
                12 or 1 or 2 => roll < 0.2f ? WeatherType.Snowy : roll < 0.5f ? WeatherType.Cloudy : WeatherType.Sunny,
                _ => roll < 0.2f ? WeatherType.Rainy : roll < 0.4f ? WeatherType.Cloudy : WeatherType.Sunny
            };
        }

        private int GetDaysInMonth(int month)
        {
            return month switch
            {
                2 => 28,
                4 or 6 or 9 or 11 => 30,
                _ => 31
            };
        }
    }
}
