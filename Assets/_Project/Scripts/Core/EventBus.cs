using System;
using System.Collections.Generic;

namespace ArcadiaOfEchoes.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();
            _handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list)) return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                ((Action<T>)list[i])?.Invoke(eventData);
            }
        }

        public static void Clear()
        {
            _handlers.Clear();
        }
    }

    // Game-wide events
    public struct PhaseChangedEvent
    {
        public GamePhase OldPhase;
        public GamePhase NewPhase;
    }

    public struct DayAdvancedEvent
    {
        public int NewDay;
        public int NewMonth;
    }

    public struct TimeSlotChangedEvent
    {
        public TimeSlot OldSlot;
        public TimeSlot NewSlot;
    }

    public struct ResonanceRankUpEvent
    {
        public string CharacterId;
        public int NewRank;
    }

    public struct BattleStartEvent
    {
        public string EncounterId;
    }

    public struct BattleEndEvent
    {
        public bool Victory;
    }
}
