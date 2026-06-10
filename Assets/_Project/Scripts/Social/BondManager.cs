using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfBacklight.Core;
using AriaOfBacklight.Data;

namespace AriaOfBacklight.Social
{
    public class BondManager : MonoBehaviour
    {
        public static BondManager Instance { get; private set; }

        [SerializeField] private List<BondData> allBonds = new();

        private Dictionary<string, int> bondRanks = new();
        private Dictionary<string, int> bondPoints = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var bond in allBonds)
            {
                bondRanks[bond.characterId] = 0;
                bondPoints[bond.characterId] = 0;
            }
        }

        public int GetRank(string characterId)
        {
            return bondRanks.TryGetValue(characterId, out var rank) ? rank : 0;
        }

        public void AddPoints(string characterId, int points)
        {
            if (!bondPoints.ContainsKey(characterId)) return;

            bondPoints[characterId] += points;
            var bond = allBonds.Find(b => b.characterId == characterId);
            if (bond == null) return;

            int currentRank = bondRanks[characterId];
            if (currentRank >= bond.maxRank) return;

            int required = bond.GetPointsForNextRank(currentRank);
            if (bondPoints[characterId] >= required)
            {
                bondPoints[characterId] -= required;
                bondRanks[characterId]++;
                EventBus.Publish(new BondRankUpEvent(characterId, bondRanks[characterId]));
            }
        }

        public bool CanRankUp(string characterId)
        {
            if (!bondRanks.ContainsKey(characterId)) return false;
            var bond = allBonds.Find(b => b.characterId == characterId);
            if (bond == null) return false;
            int currentRank = bondRanks[characterId];
            if (currentRank >= bond.maxRank) return false;
            return bondPoints[characterId] >= bond.GetPointsForNextRank(currentRank);
        }

        public bool MeetsStatRequirement(string characterId)
        {
            var bond = allBonds.Find(b => b.characterId == characterId);
            if (bond == null) return false;
            int currentRank = bondRanks[characterId];
            var req = bond.GetStatRequirement(currentRank + 1);
            if (req == null) return true;
            return PlayerStats.Instance?.GetStat(req.stat) >= req.requiredLevel;
        }
    }

    public readonly struct BondRankUpEvent
    {
        public readonly string CharacterId;
        public readonly int NewRank;
        public BondRankUpEvent(string characterId, int newRank)
        { CharacterId = characterId; NewRank = newRank; }
    }
}
