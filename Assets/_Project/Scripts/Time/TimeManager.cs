using System;
using UnityEngine;
using EchoesOfArcadia.Core;

namespace EchoesOfArcadia.Time
{
    public enum TimePeriod
    {
        Morning,
        Class,
        AfterSchool,
        Evening,
        Night
    }

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    public enum Weather
    {
        Sunny,
        Cloudy,
        Rainy,
        Storm
    }

    [Serializable]
    public struct GameDate : IEquatable<GameDate>, IComparable<GameDate>
    {
        public int Year;
        public int Month;
        public int Day;

        public GameDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public Season CurrentSeason => Month switch
        {
            >= 3 and <= 5 => Season.Spring,
            >= 6 and <= 8 => Season.Summer,
            >= 9 and <= 11 => Season.Autumn,
            _ => Season.Winter
        };

        public DayOfWeek DayOfWeek
        {
            get
            {
                var dt = new DateTime(Year, Month, Day);
                return dt.DayOfWeek;
            }
        }

        public bool IsWeekend => DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        public bool IsHoliday => IsWeekend;

        public GameDate NextDay()
        {
            var dt = new DateTime(Year, Month, Day).AddDays(1);
            return new GameDate(dt.Year, dt.Month, dt.Day);
        }

        public int DaysUntil(GameDate target)
        {
            var from = new DateTime(Year, Month, Day);
            var to = new DateTime(target.Year, target.Month, target.Day);
            return (int)(to - from).TotalDays;
        }

        public bool Equals(GameDate other) =>
            Year == other.Year && Month == other.Month && Day == other.Day;

        public int CompareTo(GameDate other)
        {
            int cmp = Year.CompareTo(other.Year);
            if (cmp != 0) return cmp;
            cmp = Month.CompareTo(other.Month);
            return cmp != 0 ? cmp : Day.CompareTo(other.Day);
        }

        public override string ToString() => $"{Year}/{Month:D2}/{Day:D2}";

        public string ToJapanese()
        {
            string[] dayNames = { "日", "月", "火", "水", "木", "金", "土" };
            return $"{Month}月{Day}日（{dayNames[(int)DayOfWeek]}）";
        }
    }

    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [SerializeField] private int startYear = 2026;
        [SerializeField] private int startMonth = 4;
        [SerializeField] private int startDay = 8;

        public GameDate CurrentDate { get; private set; }
        public TimePeriod CurrentPeriod { get; private set; }
        public Weather CurrentWeather { get; private set; }
        public int RemainingActions { get; private set; }

        public event Action<GameDate> OnDateChanged;
        public event Action<TimePeriod> OnPeriodChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentDate = new GameDate(startYear, startMonth, startDay);
            CurrentPeriod = TimePeriod.Morning;
            CurrentWeather = Weather.Sunny;
            RemainingActions = 0;
        }

        public void AdvancePeriod()
        {
            var previous = CurrentPeriod;

            switch (CurrentPeriod)
            {
                case TimePeriod.Morning:
                    CurrentPeriod = TimePeriod.Class;
                    RemainingActions = 0;
                    break;
                case TimePeriod.Class:
                    CurrentPeriod = TimePeriod.AfterSchool;
                    RemainingActions = 1;
                    break;
                case TimePeriod.AfterSchool:
                    CurrentPeriod = TimePeriod.Evening;
                    RemainingActions = 1;
                    break;
                case TimePeriod.Evening:
                    CurrentPeriod = TimePeriod.Night;
                    RemainingActions = 1;
                    break;
                case TimePeriod.Night:
                    AdvanceDay();
                    return;
            }

            OnPeriodChanged?.Invoke(CurrentPeriod);
            GameEventBus.Publish(new PeriodChangedEvent(previous, CurrentPeriod));
        }

        public void ConsumeAction()
        {
            if (RemainingActions <= 0) return;
            RemainingActions--;

            if (RemainingActions <= 0)
                AdvancePeriod();
        }

        public void AdvanceDay()
        {
            CurrentDate = CurrentDate.NextDay();
            CurrentPeriod = TimePeriod.Morning;
            RemainingActions = 0;
            CurrentWeather = GenerateWeather();

            OnDateChanged?.Invoke(CurrentDate);
            OnPeriodChanged?.Invoke(CurrentPeriod);
            GameEventBus.Publish(new DateChangedEvent(CurrentDate));
            GameEventBus.Publish(new PeriodChangedEvent(TimePeriod.Night, TimePeriod.Morning));
        }

        public void SetWeather(Weather weather)
        {
            CurrentWeather = weather;
            GameEventBus.Publish(new WeatherChangedEvent(weather));
        }

        private Weather GenerateWeather()
        {
            int month = CurrentDate.Month;
            float rainChance = month switch
            {
                6 or 7 => 0.5f,
                9 => 0.35f,
                12 or 1 or 2 => 0.15f,
                _ => 0.2f
            };

            float roll = UnityEngine.Random.value;
            if (roll < rainChance * 0.2f) return Weather.Storm;
            if (roll < rainChance) return Weather.Rainy;
            if (roll < rainChance + 0.25f) return Weather.Cloudy;
            return Weather.Sunny;
        }

        public string GetPeriodName() => CurrentPeriod switch
        {
            TimePeriod.Morning => "朝",
            TimePeriod.Class => "授業",
            TimePeriod.AfterSchool => "放課後",
            TimePeriod.Evening => "夕方",
            TimePeriod.Night => "夜",
            _ => ""
        };

        public string GetWeatherName() => CurrentWeather switch
        {
            Weather.Sunny => "晴れ",
            Weather.Cloudy => "曇り",
            Weather.Rainy => "雨",
            Weather.Storm => "嵐",
            _ => ""
        };
    }

    public readonly struct DateChangedEvent
    {
        public readonly GameDate NewDate;
        public DateChangedEvent(GameDate newDate) => NewDate = newDate;
    }

    public readonly struct PeriodChangedEvent
    {
        public readonly TimePeriod Previous;
        public readonly TimePeriod Current;
        public PeriodChangedEvent(TimePeriod previous, TimePeriod current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public readonly struct WeatherChangedEvent
    {
        public readonly Weather NewWeather;
        public WeatherChangedEvent(Weather weather) => NewWeather = weather;
    }
}
