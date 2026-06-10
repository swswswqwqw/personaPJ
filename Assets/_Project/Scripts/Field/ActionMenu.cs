using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfEchoes.Time;
using AriaOfEchoes.Core;

namespace AriaOfEchoes.Field
{
    public enum ActionCategory
    {
        Social,     // 絆・交流
        Study,      // 勉強・知識
        Activity,   // 活動・ステータスUP
        Dungeon,    // 沈黙層探索
        PartTime,   // バイト
        Rest        // 休息
    }

    [Serializable]
    public class FieldAction
    {
        public string actionName;
        public string description;
        public ActionCategory category;
        public TimePeriod availablePeriod;
        public List<Weather> requiredWeather;
        public string requiredStatName;
        public int requiredStatRank;
        public bool isAvailable;

        public Sprite icon;
    }

    public class ActionMenu : MonoBehaviour
    {
        public static ActionMenu Instance { get; private set; }

        [SerializeField] List<FieldAction> allActions = new();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public List<FieldAction> GetAvailableActions()
        {
            if (TimeManager.Instance == null) return new List<FieldAction>();

            var period = TimeManager.Instance.CurrentPeriod;
            var weather = TimeManager.Instance.CurrentWeather;
            var result = new List<FieldAction>();

            foreach (var action in allActions)
            {
                if (action.availablePeriod != period) continue;

                if (action.requiredWeather != null
                    && action.requiredWeather.Count > 0
                    && !action.requiredWeather.Contains(weather))
                    continue;

                result.Add(action);
            }

            return result;
        }

        public void ExecuteAction(FieldAction action)
        {
            EventBus.Publish(new ActionSelectedEvent(action));
        }
    }

    public struct ActionSelectedEvent
    {
        public FieldAction Action;
        public ActionSelectedEvent(FieldAction a) { Action = a; }
    }
}
