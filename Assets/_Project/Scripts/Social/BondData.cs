using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfBacklight.Data;

namespace AriaOfBacklight.Social
{
    [CreateAssetMenu(fileName = "NewBond", menuName = "AriaOfBacklight/BondData")]
    public class BondData : ScriptableObject
    {
        public string characterId;
        public string characterName;
        public ArcanaType arcana;
        public int maxRank = 10;
        public Sprite icon;

        [SerializeField] private List<RankRequirement> rankRequirements = new();
        [SerializeField] private List<StatRequirement> statRequirements = new();

        public int GetPointsForNextRank(int currentRank)
        {
            var req = rankRequirements.Find(r => r.rank == currentRank + 1);
            return req?.pointsRequired ?? (currentRank + 1) * 15;
        }

        public StatRequirement GetStatRequirement(int targetRank)
        {
            return statRequirements.Find(r => r.rank == targetRank);
        }
    }

    [Serializable]
    public class RankRequirement
    {
        public int rank;
        public int pointsRequired;
        public string dialogueId;
    }

    [Serializable]
    public class StatRequirement
    {
        public int rank;
        public PlayerStatType stat;
        public int requiredLevel;
    }
}
