using System;
using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Core;
using ArcadiaOfEchoes.Data;

namespace ArcadiaOfEchoes.Social
{
    [Serializable]
    public class BondProgress
    {
        public string CharacterId;
        public int Rank;
        public int Points;
        public bool IsAvailable;
        public bool IsMaxed;

        public const int MaxRank = 10;
        public static readonly int[] PointsPerRank = { 0, 5, 10, 15, 20, 25, 35, 40, 50, 60 };
    }

    public class ResonanceBondManager : MonoBehaviour
    {
        public static ResonanceBondManager Instance { get; private set; }

        private readonly Dictionary<string, BondProgress> _bonds = new();

        public IReadOnlyDictionary<string, BondProgress> Bonds => _bonds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterBond(CharacterData character)
        {
            if (_bonds.ContainsKey(character.CharacterId)) return;

            _bonds[character.CharacterId] = new BondProgress
            {
                CharacterId = character.CharacterId,
                Rank = 0,
                Points = 0,
                IsAvailable = false,
                IsMaxed = false
            };
        }

        public void UnlockBond(string characterId)
        {
            if (_bonds.TryGetValue(characterId, out var bond))
                bond.IsAvailable = true;
        }

        public void AddBondPoints(string characterId, int points)
        {
            if (!_bonds.TryGetValue(characterId, out var bond)) return;
            if (bond.IsMaxed) return;

            bond.Points += points;

            int threshold = bond.Rank < BondProgress.PointsPerRank.Length
                ? BondProgress.PointsPerRank[bond.Rank]
                : 99;

            if (bond.Points >= threshold)
            {
                bond.Points -= threshold;
                bond.Rank++;

                if (bond.Rank >= BondProgress.MaxRank)
                {
                    bond.IsMaxed = true;
                    bond.Rank = BondProgress.MaxRank;
                }

                EventBus.Publish(new BondRankUpEvent(characterId, bond.Rank));
            }
        }

        public int GetBondRank(string characterId)
        {
            return _bonds.TryGetValue(characterId, out var bond) ? bond.Rank : 0;
        }

        public bool IsBondMaxed(string characterId)
        {
            return _bonds.TryGetValue(characterId, out var bond) && bond.IsMaxed;
        }

        public float GetUnisonBreakMultiplier()
        {
            float totalBonus = 1.0f;
            foreach (var bond in _bonds.Values)
                totalBonus += bond.Rank * 0.02f;
            return totalBonus;
        }
    }

    public readonly struct BondRankUpEvent
    {
        public readonly string CharacterId;
        public readonly int NewRank;
        public BondRankUpEvent(string id, int rank) { CharacterId = id; NewRank = rank; }
    }
}
