using System;
using System.Collections.Generic;

namespace AriaOfEchoes.Core
{
    public static class EventBus
    {
        static readonly Dictionary<Type, List<Delegate>> listeners = new();

        public static void Subscribe<T>(Action<T> callback) where T : struct
        {
            var type = typeof(T);
            if (!listeners.ContainsKey(type))
                listeners[type] = new List<Delegate>();
            listeners[type].Add(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : struct
        {
            var type = typeof(T);
            if (listeners.ContainsKey(type))
                listeners[type].Remove(callback);
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!listeners.ContainsKey(type)) return;

            for (int i = listeners[type].Count - 1; i >= 0; i--)
            {
                if (listeners[type][i] is Action<T> callback)
                    callback(eventData);
            }
        }

        public static void Clear()
        {
            listeners.Clear();
        }
    }
}
