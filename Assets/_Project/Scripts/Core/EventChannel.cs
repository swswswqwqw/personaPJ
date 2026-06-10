using System;
using System.Collections.Generic;

namespace Amane.Core
{
    /// <summary>
    /// 型安全な疎結合イベントバス。Plain C#。
    /// ScriptableObject ベースの EventChannel へ後日差し替え可能なよう、
    /// まずはコードベースのバスとして最小実装する。
    /// </summary>
    public sealed class EventChannel
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        /// <summary>イベントを購読する。返り値の IDisposable で解除する。</summary>
        public IDisposable Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var key = typeof(T);
            _handlers[key] = _handlers.TryGetValue(key, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
            return new Subscription(() => Unsubscribe(handler));
        }

        /// <summary>イベントを発行する。購読者がいなくても安全。</summary>
        public void Publish<T>(in T message) where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var d) && d is Action<T> action)
                action(message);
        }

        private void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var key = typeof(T);
            if (!_handlers.TryGetValue(key, out var existing)) return;
            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null) _handlers.Remove(key);
            else _handlers[key] = remaining;
        }

        private sealed class Subscription : IDisposable
        {
            private Action _dispose;
            public Subscription(Action dispose) => _dispose = dispose;
            public void Dispose()
            {
                _dispose?.Invoke();
                _dispose = null;
            }
        }
    }

    // ---- ゲーム内イベント定義（struct: GC負荷を避ける） ----

    /// <summary>時間帯が進んだ。</summary>
    public readonly struct TimeAdvancedEvent
    {
        public readonly int Day;
        public readonly Amane.Time.TimeSlot Slot;
        public TimeAdvancedEvent(int day, Amane.Time.TimeSlot slot) { Day = day; Slot = slot; }
    }

    /// <summary>日付が変わった。</summary>
    public readonly struct DayChangedEvent
    {
        public readonly int Day;
        public readonly Amane.Time.Weather Weather;
        public DayChangedEvent(int day, Amane.Time.Weather weather) { Day = day; Weather = weather; }
    }

    /// <summary>デッドラインが迫っている（残り日数つき）。</summary>
    public readonly struct DeadlineApproachingEvent
    {
        public readonly string CaseId;
        public readonly int DaysLeft;
        public DeadlineApproachingEvent(string caseId, int daysLeft) { CaseId = caseId; DaysLeft = daysLeft; }
    }

    /// <summary>絆ランクが上がった。</summary>
    public readonly struct BondRankUpEvent
    {
        public readonly string BondId;
        public readonly int NewRank;
        public BondRankUpEvent(string bondId, int newRank) { BondId = bondId; NewRank = newRank; }
    }

    /// <summary>カレンダーイベントが発生した（月初事件・満月・テスト等）。</summary>
    public readonly struct CalendarEventTriggered
    {
        public readonly string EventId;
        public readonly string DisplayName;
        public readonly Amane.Time.CalendarEventType Type;
        public CalendarEventTriggered(string eventId, string displayName, Amane.Time.CalendarEventType type)
        {
            EventId = eventId;
            DisplayName = displayName;
            Type = type;
        }
    }

    /// <summary>内面ステータスがランクアップした。</summary>
    public readonly struct StatRankUpEvent
    {
        public readonly Amane.Stat.InnerStat Stat;
        public readonly int NewRank;
        public StatRankUpEvent(Amane.Stat.InnerStat stat, int newRank) { Stat = stat; NewRank = newRank; }
    }
}
