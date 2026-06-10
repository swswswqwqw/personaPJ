using System.Collections.Generic;
using UnityEngine;
using ArcanaOfHollows.Core;
using ArcanaOfHollows.Data;

namespace ArcanaOfHollows.Social
{
    public class HeartStringManager : MonoBehaviour
    {
        public static HeartStringManager Instance { get; private set; }

        public const int MaxRank = 10;

        private Dictionary<string, HeartStringState> heartStrings = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterHeartString(HeartStringData data)
        {
            if (heartStrings.ContainsKey(data.characterId)) return;
            heartStrings[data.characterId] = new HeartStringState(data);
        }

        public int GetRank(string characterId)
        {
            return heartStrings.TryGetValue(characterId, out var state) ? state.Rank : 0;
        }

        public float GetPoints(string characterId)
        {
            return heartStrings.TryGetValue(characterId, out var state) ? state.Points : 0;
        }

        public bool IsUnlocked(string characterId)
        {
            return heartStrings.TryGetValue(characterId, out var state) && state.IsUnlocked;
        }

        public void Unlock(string characterId)
        {
            if (!heartStrings.TryGetValue(characterId, out var state)) return;
            if (state.IsUnlocked) return;

            state.IsUnlocked = true;
            EventBus.Publish(new HeartStringUnlockedEvent(characterId, state.Data));
        }

        public void AddPoints(string characterId, int points)
        {
            if (!heartStrings.TryGetValue(characterId, out var state)) return;
            if (!state.IsUnlocked || state.Rank >= MaxRank) return;

            state.Points += points;
            EventBus.Publish(new HeartStringPointsGainedEvent(characterId, points, state.Points));

            int requiredPoints = GetRequiredPointsForNextRank(state.Rank);
            if (state.Points >= requiredPoints)
            {
                RankUp(characterId, state);
            }
        }

        private void RankUp(string characterId, HeartStringState state)
        {
            state.Rank++;
            state.Points = 0;

            EventBus.Publish(new HeartStringRankUpEvent(characterId, state.Rank, state.Data));

            var reward = state.Data.GetRewardForRank(state.Rank);
            if (reward != null)
            {
                EventBus.Publish(new HeartStringRewardEvent(characterId, reward));
            }
        }

        public List<HeartStringSummary> GetAllHeartStrings()
        {
            var summaries = new List<HeartStringSummary>();
            foreach (var kvp in heartStrings)
            {
                summaries.Add(new HeartStringSummary
                {
                    CharacterId = kvp.Key,
                    CharacterName = kvp.Value.Data.characterName,
                    Arcana = kvp.Value.Data.arcana,
                    Rank = kvp.Value.Rank,
                    IsUnlocked = kvp.Value.IsUnlocked
                });
            }
            return summaries;
        }

        private int GetRequiredPointsForNextRank(int currentRank)
        {
            return currentRank switch
            {
                0 => 0,
                1 => 2,
                2 => 3,
                3 => 3,
                4 => 4,
                5 => 5,
                6 => 5,
                7 => 6,
                8 => 6,
                9 => 8,
                _ => 999
            };
        }
    }

    public class HeartStringState
    {
        public HeartStringData Data { get; }
        public int Rank { get; set; }
        public float Points { get; set; }
        public bool IsUnlocked { get; set; }

        public HeartStringState(HeartStringData data)
        {
            Data = data;
            Rank = 0;
            Points = 0;
            IsUnlocked = false;
        }
    }

    public struct HeartStringSummary
    {
        public string CharacterId;
        public string CharacterName;
        public Arcana Arcana;
        public int Rank;
        public bool IsUnlocked;
    }

    public readonly struct HeartStringUnlockedEvent
    {
        public readonly string CharacterId;
        public readonly HeartStringData Data;

        public HeartStringUnlockedEvent(string characterId, HeartStringData data)
        {
            CharacterId = characterId;
            Data = data;
        }
    }

    public readonly struct HeartStringPointsGainedEvent
    {
        public readonly string CharacterId;
        public readonly int PointsGained;
        public readonly float TotalPoints;

        public HeartStringPointsGainedEvent(string characterId, int pointsGained, float totalPoints)
        {
            CharacterId = characterId;
            PointsGained = pointsGained;
            TotalPoints = totalPoints;
        }
    }

    public readonly struct HeartStringRankUpEvent
    {
        public readonly string CharacterId;
        public readonly int NewRank;
        public readonly HeartStringData Data;

        public HeartStringRankUpEvent(string characterId, int newRank, HeartStringData data)
        {
            CharacterId = characterId;
            NewRank = newRank;
            Data = data;
        }
    }

    public readonly struct HeartStringRewardEvent
    {
        public readonly string CharacterId;
        public readonly HeartStringReward Reward;

        public HeartStringRewardEvent(string characterId, HeartStringReward reward)
        {
            CharacterId = characterId;
            Reward = reward;
        }
    }
}
