using System;
using UnityEngine;
using ArcadiaOfEchoes.Core;

namespace ArcadiaOfEchoes.Time
{
    public enum TimePeriod
    {
        Morning,     // 朝 — 登校
        Class,       // 昼 — 授業
        AfterSchool, // 放課後 — 黄昏
        Evening,     // 夜 — 宵闇
        LateNight    // 深夜 — 残響時
    }

    public enum Weather
    {
        Sunny,
        Cloudy,
        Rainy,
        Storm
    }

    [Serializable]
    public struct GameDate : IEquatable<GameDate>
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

        public bool Equals(GameDate other) =>
            Year == other.Year && Month == other.Month && Day == other.Day;

        public override bool Equals(object obj) => obj is GameDate other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Year, Month, Day);
        public override string ToString() => $"{Year}/{Month:D2}/{Day:D2}";

        public static bool operator ==(GameDate a, GameDate b) => a.Equals(b);
        public static bool operator !=(GameDate a, GameDate b) => !a.Equals(b);
    }

    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        public GameDate CurrentDate { get; private set; }
        public TimePeriod CurrentPeriod { get; private set; }
        public Weather CurrentWeather { get; private set; }
        public int ActionPointsRemaining { get; private set; }

        private static readonly GameDate GameStartDate = new(2026, 4, 8);
        private static readonly GameDate GameEndDate = new(2027, 3, 15);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        {
            CurrentDate = GameStartDate;
            CurrentPeriod = TimePeriod.Morning;
            CurrentWeather = Weather.Sunny;
            ActionPointsRemaining = 0;
            EventBus.Publish(new DateChangedEvent(CurrentDate, CurrentPeriod));
        }

        public void AdvancePeriod()
        {
            var previousPeriod = CurrentPeriod;

            switch (CurrentPeriod)
            {
                case TimePeriod.Morning:
                    CurrentPeriod = TimePeriod.Class;
                    ActionPointsRemaining = 0;
                    break;
                case TimePeriod.Class:
                    CurrentPeriod = TimePeriod.AfterSchool;
                    ActionPointsRemaining = 1;
                    break;
                case TimePeriod.AfterSchool:
                    CurrentPeriod = TimePeriod.Evening;
                    ActionPointsRemaining = 1;
                    break;
                case TimePeriod.Evening:
                    CurrentPeriod = TimePeriod.LateNight;
                    ActionPointsRemaining = IsEchoNight() ? 1 : 0;
                    break;
                case TimePeriod.LateNight:
                    AdvanceDay();
                    return;
            }

            EventBus.Publish(new TimePeriodChangedEvent(previousPeriod, CurrentPeriod));
        }

        public void ConsumeActionPoint()
        {
            if (ActionPointsRemaining <= 0) return;
            ActionPointsRemaining--;
            EventBus.Publish(new ActionPointConsumedEvent(ActionPointsRemaining));
        }

        private void AdvanceDay()
        {
            var previousDate = CurrentDate;
            int day = CurrentDate.Day + 1;
            int month = CurrentDate.Month;
            int year = CurrentDate.Year;

            int daysInMonth = DateTime.DaysInMonth(year, month);
            if (day > daysInMonth)
            {
                day = 1;
                month++;
                if (month > 12)
                {
                    month = 1;
                    year++;
                }
            }

            CurrentDate = new GameDate(year, month, day);
            CurrentPeriod = TimePeriod.Morning;
            CurrentWeather = GenerateWeather();
            ActionPointsRemaining = 0;

            EventBus.Publish(new DateChangedEvent(CurrentDate, CurrentPeriod));

            if (CurrentDate == GameEndDate)
                EventBus.Publish(new GameEndReachedEvent());
        }

        private bool IsEchoNight()
        {
            // 満月の夜や特定の日に残響時が発生する
            return CurrentDate.Day == 15 || CurrentDate.Day == 1;
        }

        private Weather GenerateWeather()
        {
            int roll = UnityEngine.Random.Range(0, 100);
            int month = CurrentDate.Month;

            // 梅雨(6月)は雨が多い、冬は晴れが多い
            if (month == 6) return roll < 60 ? Weather.Rainy : roll < 80 ? Weather.Cloudy : Weather.Sunny;
            if (month >= 12 || month <= 2) return roll < 70 ? Weather.Sunny : roll < 90 ? Weather.Cloudy : Weather.Rainy;
            return roll < 50 ? Weather.Sunny : roll < 80 ? Weather.Cloudy : roll < 95 ? Weather.Rainy : Weather.Storm;
        }

        public bool HasActionPoints() => ActionPointsRemaining > 0;

        public bool IsDeadlineClose(GameDate deadline, int warningDays = 3)
        {
            var deadlineDt = new DateTime(deadline.Year, deadline.Month, deadline.Day);
            var currentDt = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day);
            return (deadlineDt - currentDt).TotalDays <= warningDays && (deadlineDt - currentDt).TotalDays >= 0;
        }
    }

    public readonly struct DateChangedEvent
    {
        public readonly GameDate NewDate;
        public readonly TimePeriod Period;
        public DateChangedEvent(GameDate date, TimePeriod period) { NewDate = date; Period = period; }
    }

    public readonly struct TimePeriodChangedEvent
    {
        public readonly TimePeriod Previous;
        public readonly TimePeriod Current;
        public TimePeriodChangedEvent(TimePeriod prev, TimePeriod curr) { Previous = prev; Current = curr; }
    }

    public readonly struct ActionPointConsumedEvent
    {
        public readonly int Remaining;
        public ActionPointConsumedEvent(int remaining) { Remaining = remaining; }
    }

    public readonly struct GameEndReachedEvent { }
}
