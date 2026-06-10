using System;
using UnityEngine;
using AriaOfEchoes.Core;

namespace AriaOfEchoes.Time
{
    public enum TimePeriod
    {
        Morning,    // 朝（通学）
        Class,      // 授業中
        Lunch,      // 昼休み
        AfterSchool,// 放課後
        Evening,    // 夜
        LateNight   // 深夜
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

        [SerializeField] CalendarData calendarData;

        GameDate currentDate;
        TimePeriod currentPeriod;
        Weather currentWeather;

        public GameDate CurrentDate => currentDate;
        public TimePeriod CurrentPeriod => currentPeriod;
        public Weather CurrentWeather => currentWeather;

        public event Action<TimePeriod> OnPeriodChanged;
        public event Action<GameDate> OnDateChanged;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            EventBus.Subscribe<NewGameEvent>(OnNewGame);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<NewGameEvent>(OnNewGame);
        }

        void OnNewGame(NewGameEvent e)
        {
            currentDate = new GameDate(2025, 4, 8); // 始業式
            currentPeriod = TimePeriod.Morning;
            currentWeather = Weather.Sunny;
            OnDateChanged?.Invoke(currentDate);
            OnPeriodChanged?.Invoke(currentPeriod);
        }

        public void AdvancePeriod()
        {
            var oldPeriod = currentPeriod;
            currentPeriod = GetNextPeriod(currentPeriod);

            if (currentPeriod == TimePeriod.Morning)
            {
                AdvanceDay();
            }

            OnPeriodChanged?.Invoke(currentPeriod);
            EventBus.Publish(new PeriodChangedEvent(oldPeriod, currentPeriod));
        }

        void AdvanceDay()
        {
            var oldDate = currentDate;
            currentDate = currentDate.NextDay();
            currentWeather = DetermineWeather(currentDate);
            OnDateChanged?.Invoke(currentDate);
            EventBus.Publish(new DateChangedEvent(oldDate, currentDate));
        }

        public int GetActionPoints(TimePeriod period)
        {
            return period switch
            {
                TimePeriod.Morning => 0,
                TimePeriod.Class => 0,
                TimePeriod.Lunch => 1,
                TimePeriod.AfterSchool => 1,
                TimePeriod.Evening => 1,
                TimePeriod.LateNight => 0,
                _ => 0
            };
        }

        public bool IsActionPeriod(TimePeriod period)
        {
            return GetActionPoints(period) > 0;
        }

        public bool IsDeadlineApproaching(int daysRemaining)
        {
            return daysRemaining <= 3;
        }

        TimePeriod GetNextPeriod(TimePeriod current)
        {
            return current switch
            {
                TimePeriod.Morning => TimePeriod.Class,
                TimePeriod.Class => TimePeriod.Lunch,
                TimePeriod.Lunch => TimePeriod.AfterSchool,
                TimePeriod.AfterSchool => TimePeriod.Evening,
                TimePeriod.Evening => TimePeriod.LateNight,
                TimePeriod.LateNight => TimePeriod.Morning,
                _ => TimePeriod.Morning
            };
        }

        Weather DetermineWeather(GameDate date)
        {
            // 梅雨（6月）は雨が多い
            if (date.Month == 6)
            {
                var hash = HashDate(date);
                return (hash % 3 == 0) ? Weather.Sunny : Weather.Rainy;
            }

            var dayHash = HashDate(date);
            return (dayHash % 5) switch
            {
                0 => Weather.Rainy,
                1 => Weather.Cloudy,
                _ => Weather.Sunny
            };
        }

        int HashDate(GameDate date)
        {
            return date.Year * 10000 + date.Month * 100 + date.Day;
        }
    }

    public struct PeriodChangedEvent
    {
        public TimePeriod OldPeriod { get; }
        public TimePeriod NewPeriod { get; }

        public PeriodChangedEvent(TimePeriod oldPeriod, TimePeriod newPeriod)
        {
            OldPeriod = oldPeriod;
            NewPeriod = newPeriod;
        }
    }

    public struct DateChangedEvent
    {
        public GameDate OldDate { get; }
        public GameDate NewDate { get; }

        public DateChangedEvent(GameDate oldDate, GameDate newDate)
        {
            OldDate = oldDate;
            NewDate = newDate;
        }
    }
}
