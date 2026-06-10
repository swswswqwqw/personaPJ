using System;
using System.Collections.Generic;

namespace AriaOfBacklight.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Handlers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!Handlers.ContainsKey(type))
                Handlers[type] = new List<Delegate>();
            Handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (Handlers.ContainsKey(type))
                Handlers[type].Remove(handler);
        }

        public static void Publish<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (!Handlers.TryGetValue(type, out var handlers))
                return;
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                if (handlers[i] is Action<T> action)
                    action.Invoke(evt);
            }
        }

        public static void Clear()
        {
            Handlers.Clear();
        }
    }
}
