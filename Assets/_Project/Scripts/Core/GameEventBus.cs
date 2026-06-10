using System;
using System.Collections.Generic;

namespace Amane.Core
{
    public static class GameEventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Listeners = new();

        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            var type = typeof(T);
            if (!Listeners.ContainsKey(type))
                Listeners[type] = new List<Delegate>();

            Listeners[type].Add(listener);
        }

        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            var type = typeof(T);
            if (Listeners.ContainsKey(type))
                Listeners[type].Remove(listener);
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!Listeners.ContainsKey(type)) return;

            foreach (var listener in Listeners[type].ToArray())
            {
                ((Action<T>)listener)?.Invoke(eventData);
            }
        }

        public static void Clear()
        {
            Listeners.Clear();
        }
    }
}
