using System;
using System.Collections.Generic;

namespace ArcanaOfHollows.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscribers = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!Subscribers.ContainsKey(type))
                Subscribers[type] = new List<Delegate>();
            Subscribers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (Subscribers.ContainsKey(type))
                Subscribers[type].Remove(handler);
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (!Subscribers.ContainsKey(type)) return;

            foreach (var subscriber in Subscribers[type].ToArray())
            {
                ((Action<T>)subscriber)?.Invoke(eventData);
            }
        }

        public static void Clear()
        {
            Subscribers.Clear();
        }
    }
}
