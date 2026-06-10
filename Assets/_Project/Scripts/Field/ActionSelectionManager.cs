using System.Collections.Generic;
using UnityEngine;
using ArcanaOfHollows.Core;
using ArcanaOfHollows.Time;
using ArcanaOfHollows.Player;

namespace ArcanaOfHollows.Field
{
    public enum ActionType
    {
        HeartString,
        Study,
        PartTimeJob,
        Training,
        Shopping,
        Fishing,
        DungeonExplore,
        SpecialEvent,
        Rest
    }

    public class ActionSelectionManager : MonoBehaviour
    {
        public static ActionSelectionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public List<AvailableAction> GetAvailableActions()
        {
            var actions = new List<AvailableAction>();
            var timeManager = TimeManager.Instance;
            if (timeManager == null) return actions;

            var period = timeManager.CurrentPeriod;

            if (period == TimePeriod.AfterSchool)
            {
                actions.Add(new AvailableAction(ActionType.HeartString, "誰かと過ごす", "放課後の時間を仲間と過ごす"));
                actions.Add(new AvailableAction(ActionType.Study, "図書館で勉強", "洞察が上がる", SocialStat.Insight));
                actions.Add(new AvailableAction(ActionType.Training, "運動する", "忍耐が上がる", SocialStat.Patience));
                actions.Add(new AvailableAction(ActionType.Shopping, "買い物に行く", "装備やアイテムを購入"));
                actions.Add(new AvailableAction(ActionType.DungeonExplore, "空蝉界に潜る", "ダンジョンを探索する"));
            }
            else if (period == TimePeriod.Evening)
            {
                actions.Add(new AvailableAction(ActionType.HeartString, "誰かと過ごす", "夜の時間を仲間と過ごす"));
                actions.Add(new AvailableAction(ActionType.Study, "自室で勉強", "洞察が上がる", SocialStat.Insight));
                actions.Add(new AvailableAction(ActionType.PartTimeJob, "バイトに行く", "話術が上がり、お金も稼げる", SocialStat.Eloquence));
                actions.Add(new AvailableAction(ActionType.Fishing, "夜釣り", "忍耐が上がる", SocialStat.Patience));
                actions.Add(new AvailableAction(ActionType.DungeonExplore, "空蝉界に潜る", "ダンジョンを探索する"));
                actions.Add(new AvailableAction(ActionType.Rest, "早く寝る", "明日に備えて休む"));
            }

            if (StoryFlagManager.Instance != null)
            {
                actions.RemoveAll(a =>
                    a.Type == ActionType.DungeonExplore &&
                    !StoryFlagManager.Instance.HasFlag("hollow_unlocked"));
            }

            return actions;
        }

        public void ExecuteAction(AvailableAction action)
        {
            EventBus.Publish(new ActionSelectedEvent(action));

            switch (action.Type)
            {
                case ActionType.Study:
                    PlayerStats.Instance?.AddStatPoints(SocialStat.Insight, 2);
                    break;
                case ActionType.Training:
                    PlayerStats.Instance?.AddStatPoints(SocialStat.Patience, 2);
                    break;
                case ActionType.PartTimeJob:
                    PlayerStats.Instance?.AddStatPoints(SocialStat.Eloquence, 1);
                    PlayerStats.Instance?.AddGold(3000);
                    break;
                case ActionType.Fishing:
                    PlayerStats.Instance?.AddStatPoints(SocialStat.Patience, 1);
                    break;
                case ActionType.Rest:
                    break;
            }

            TimeManager.Instance?.AdvancePeriod();
        }
    }

    public class AvailableAction
    {
        public ActionType Type { get; }
        public string Name { get; }
        public string Description { get; }
        public SocialStat? StatBoost { get; }

        public AvailableAction(ActionType type, string name, string description, SocialStat? statBoost = null)
        {
            Type = type;
            Name = name;
            Description = description;
            StatBoost = statBoost;
        }
    }

    public readonly struct ActionSelectedEvent
    {
        public readonly AvailableAction Action;
        public ActionSelectedEvent(AvailableAction action) { Action = action; }
    }
}
