using System;
using System.Collections.Generic;
using UnityEngine;
using ArcanaOfHollows.Data;

namespace ArcanaOfHollows.Social
{
    [CreateAssetMenu(fileName = "NewHeartString", menuName = "ArcanaOfHollows/Heart String Data")]
    public class HeartStringData : ScriptableObject
    {
        [Header("Character")]
        public string characterId;
        public string characterName;
        public Arcana arcana;
        public Sprite portrait;
        [TextArea(2, 4)] public string introduction;

        [Header("Availability")]
        public DayAvailability[] availability;

        [Header("Rank Events")]
        public HeartStringRankEvent[] rankEvents;

        [Header("Rewards")]
        public HeartStringReward[] rewards;

        public HeartStringReward GetRewardForRank(int rank)
        {
            foreach (var reward in rewards)
            {
                if (reward.rank == rank)
                    return reward;
            }
            return null;
        }

        public string GetDialogueIdForRank(int rank)
        {
            foreach (var rankEvent in rankEvents)
            {
                if (rankEvent.rank == rank)
                    return rankEvent.dialogueSequenceId;
            }
            return null;
        }
    }

    [Serializable]
    public class DayAvailability
    {
        public string dayOfWeek;
        public Time.TimePeriod timePeriod;
        public string location;
        public int requiredMonth;
    }

    [Serializable]
    public class HeartStringRankEvent
    {
        public int rank;
        public string dialogueSequenceId;
        [TextArea(2, 4)] public string synopsis;
        public string requiredStat;
        public int requiredStatLevel;
    }

    [Serializable]
    public class HeartStringReward
    {
        public int rank;
        public string rewardName;
        [TextArea(1, 2)] public string description;
        public HeartStringRewardType rewardType;
        public float value;
    }

    public enum HeartStringRewardType
    {
        SkillUnlock,
        StatBoost,
        PassiveAbility,
        EchoEvolution,
        SpecialItem
    }
}
