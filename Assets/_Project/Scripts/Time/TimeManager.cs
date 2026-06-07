using System;
using UnityEngine;
using EchoesOfArcadia.Core;

namespace EchoesOfArcadia.TimeSystem
{
    public enum TimeOfDay
    {
        Morning,
        Class,
        Afternoon,
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

    public enum Weather
    {
        Sunny,
        Cloudy,
        Rainy,
        Stormy
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

        public Season CurrentSeason
        {
            get
            {
                return Month switch
                {
                    >= 3 and <= 5 => Season.Spring,
                    >= 6 and <= 8 => Season.Summer,
                    >= 9 and <= 11 => Season.Autumn,
                    _ => Season.Winter
                };
            }
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                var dt = new DateTime(Year, Month, Day);
                return dt.DayOfWeek;
            }
        }

        public bool IsWeekend => DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        public GameDate NextDay()
        {
            var dt = new DateTime(Year, Month, Day).AddDays(1);
            return new GameDate(dt.Year, dt.Month, dt.Day);
        }

        public int TotalDays => (int)(new DateTime(Year, Month, Day) - new DateTime(2025, 4, 1)).TotalDays;

        public bool Equals(GameDate other) => Year == other.Year && Month == other.Month && Day == other.Day;
        public int CompareTo(GameDate other) => TotalDays.CompareTo(other.TotalDays);
        public override bool Equals(object obj) => obj is GameDate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Year, Month, Day);
        public override string ToString() => $"{Year}/{Month:D2}/{Day:D2}";

        public static bool operator ==(GameDate a, GameDate b) => a.Equals(b);
        public static bool operator !=(GameDate a, GameDate b) => !a.Equals(b);
        public static bool operator <(GameDate a, GameDate b) => a.CompareTo(b) < 0;
        public static bool operator >(GameDate a, GameDate b) => a.CompareTo(b) > 0;
        public static bool operator <=(GameDate a, GameDate b) => a.CompareTo(b) <= 0;
        public static bool operator >=(GameDate a, GameDate b) => a.CompareTo(b) >= 0;
    }

    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [SerializeField] private int startYear = 2025;
        [SerializeField] private int startMonth = 4;
        [SerializeField] private int startDay = 8;

        public GameDate CurrentDate { get; private set; }
        public TimeOfDay CurrentTimeOfDay { get; private set; }
        public Weather CurrentWeather { get; private set; }
        public int RemainingActionPoints { get; private set; }

        public event Action<GameDate> OnDateChanged;
        public event Action<TimeOfDay> OnTimeOfDayChanged;

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
            CurrentTimeOfDay = TimeOfDay.Morning;
            RollWeather();
        }

        public void AdvanceTimeOfDay()
        {
            var oldTime = CurrentTimeOfDay;
            CurrentTimeOfDay = oldTime switch
            {
                TimeOfDay.Morning => TimeOfDay.Class,
                TimeOfDay.Class => TimeOfDay.Afternoon,
                TimeOfDay.Afternoon => TimeOfDay.Evening,
                TimeOfDay.Evening => TimeOfDay.LateNight,
                TimeOfDay.LateNight => TimeOfDay.Morning,
                _ => TimeOfDay.Morning
            };

            if (CurrentTimeOfDay == TimeOfDay.Morning)
            {
                AdvanceDay();
            }
            else
            {
                RefreshActionPoints();
                OnTimeOfDayChanged?.Invoke(CurrentTimeOfDay);
                GameEventBus.Publish(new TimeAdvancedEvent(CurrentDate, CurrentTimeOfDay));
            }
        }

        public bool SpendActionPoint()
        {
            if (RemainingActionPoints <= 0) return false;
            RemainingActionPoints--;
            return true;
        }

        public bool CanActInCurrentTimeSlot()
        {
            return CurrentTimeOfDay switch
            {
                TimeOfDay.Morning => false,
                TimeOfDay.Class => false,
                TimeOfDay.Afternoon => RemainingActionPoints > 0,
                TimeOfDay.Evening => RemainingActionPoints > 0,
                TimeOfDay.LateNight => RemainingActionPoints > 0 && HasEnoughEndurance(),
                _ => false
            };
        }

        public int DaysUntilDeadline(GameDate deadline)
        {
            return deadline.TotalDays - CurrentDate.TotalDays;
        }

        private void AdvanceDay()
        {
            CurrentDate = CurrentDate.NextDay();
            CurrentTimeOfDay = TimeOfDay.Morning;
            RollWeather();
            RefreshActionPoints();

            OnDateChanged?.Invoke(CurrentDate);
            OnTimeOfDayChanged?.Invoke(CurrentTimeOfDay);
            GameEventBus.Publish(new DayChangedEvent(CurrentDate));
            GameEventBus.Publish(new TimeAdvancedEvent(CurrentDate, CurrentTimeOfDay));
        }

        private void RefreshActionPoints()
        {
            RemainingActionPoints = CurrentTimeOfDay switch
            {
                TimeOfDay.Afternoon => 1,
                TimeOfDay.Evening => 1,
                TimeOfDay.LateNight => HasEnoughEndurance() ? 1 : 0,
                _ => 0
            };
        }

        private void RollWeather()
        {
            float roll = UnityEngine.Random.value;
            CurrentWeather = CurrentDate.CurrentSeason switch
            {
                Season.Summer => roll < 0.3f ? Weather.Rainy : roll < 0.5f ? Weather.Cloudy : Weather.Sunny,
                Season.Winter => roll < 0.2f ? Weather.Stormy : roll < 0.5f ? Weather.Cloudy : Weather.Sunny,
                _ => roll < 0.15f ? Weather.Rainy : roll < 0.35f ? Weather.Cloudy : Weather.Sunny
            };
        }

        private bool HasEnoughEndurance()
        {
            // TODO: PlayerStatsManagerの忍耐ステータスをチェック
            return false;
        }
    }

    public readonly struct TimeAdvancedEvent
    {
        public readonly GameDate Date;
        public readonly TimeOfDay TimeOfDay;

        public TimeAdvancedEvent(GameDate date, TimeOfDay timeOfDay)
        {
            Date = date;
            TimeOfDay = timeOfDay;
        }
    }

    public readonly struct DayChangedEvent
    {
        public readonly GameDate NewDate;

        public DayChangedEvent(GameDate newDate)
        {
            NewDate = newDate;
        }
    }
}
