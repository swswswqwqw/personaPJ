using System;
using System.Collections.Generic;

namespace EchoesOfArcadia.Core
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

            for (int i = Listeners[type].Count - 1; i >= 0; i--)
            {
                if (Listeners[type][i] is Action<T> callback)
                    callback.Invoke(eventData);
            }
        }

        public static void Clear()
        {
            Listeners.Clear();
        }

        public static void Clear<T>() where T : struct
        {
            var type = typeof(T);
            if (Listeners.ContainsKey(type))
                Listeners[type].Clear();
        }
    }
}
