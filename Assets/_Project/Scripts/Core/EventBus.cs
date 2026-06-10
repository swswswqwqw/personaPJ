using System;
using System.Collections.Generic;

namespace Astra.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();
            _handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
                _handlers[type].Remove(handler);
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type)) return;

            foreach (var handler in _handlers[type].ToArray())
            {
                (handler as Action<T>)?.Invoke(eventData);
            }
        }

        public static void Clear()
        {
            _handlers.Clear();
        }
    }
}
