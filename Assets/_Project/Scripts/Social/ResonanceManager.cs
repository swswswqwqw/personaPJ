using UnityEngine;
using System.Collections.Generic;
using Astra.Core;
using Astra.Data;

namespace Astra.Social
{
    [System.Serializable]
    public class ResonanceLink
    {
        public CharacterData character;
        public int rank;
        public int points;
        public bool isAvailable;
        public bool isMaxed;

        private static readonly int[] PointsToRankUp = { 0, 2, 3, 4, 5, 6, 8, 10, 12, 15 };

        public int PointsNeeded => rank < PointsToRankUp.Length ? PointsToRankUp[rank] : 99;

        public bool CanRankUp => points >= PointsNeeded && rank < 10;
    }

    public class ResonanceManager : MonoBehaviour
    {
        public static ResonanceManager Instance { get; private set; }

        [SerializeField] private List<ResonanceLink> links = new();

        public IReadOnlyList<ResonanceLink> Links => links;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public ResonanceLink GetLink(CharacterData character)
        {
            return links.Find(l => l.character == character);
        }

        public void AddPoints(CharacterData character, int amount)
        {
            var link = GetLink(character);
            if (link == null || link.isMaxed) return;

            link.points += amount;
            EventBus.Publish(new ResonancePointsGainedEvent(character, amount, link.points));

            if (link.CanRankUp)
                RankUp(link);
        }

        private void RankUp(ResonanceLink link)
        {
            link.points -= link.PointsNeeded;
            link.rank++;

            if (link.rank >= 10)
                link.isMaxed = true;

            link.character.resonanceLinkRank = link.rank;
            EventBus.Publish(new ResonanceRankUpEvent(link.character, link.rank));
        }

        public float GetBurstDamageMultiplier()
        {
            float multiplier = 1.0f;
            foreach (var link in links)
            {
                if (link.rank >= 5) multiplier += 0.2f;
                if (link.rank >= 7) multiplier += 0.1f;
                if (link.rank >= 10) multiplier += 0.2f;
            }
            return multiplier;
        }

        public int GetAverageRank()
        {
            if (links.Count == 0) return 0;
            int total = 0;
            foreach (var link in links)
                total += link.rank;
            return total / links.Count;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public readonly struct ResonancePointsGainedEvent
    {
        public readonly CharacterData Character;
        public readonly int Amount;
        public readonly int TotalPoints;
        public ResonancePointsGainedEvent(CharacterData c, int a, int t) { Character = c; Amount = a; TotalPoints = t; }
    }

    public readonly struct ResonanceRankUpEvent
    {
        public readonly CharacterData Character;
        public readonly int NewRank;
        public ResonanceRankUpEvent(CharacterData c, int r) { Character = c; NewRank = r; }
    }
}
