using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Social
{
    [Serializable]
    public class BondProgress
    {
        public CharacterData character;
        public int currentRank;
        public int currentPoints;
        public bool isReversed;
        public bool isMaxed;

        public int PointsToNextRank => GetRequiredPoints(currentRank + 1);

        private static int GetRequiredPoints(int rank)
        {
            return rank switch
            {
                1 => 0,
                2 => 15,
                3 => 15,
                4 => 22,
                5 => 22,
                6 => 30,
                7 => 30,
                8 => 40,
                9 => 40,
                10 => 55,
                _ => 999
            };
        }
    }

    public class SocialLinkManager : MonoBehaviour
    {
        public static SocialLinkManager Instance { get; private set; }

        private readonly Dictionary<Arcana, BondProgress> bonds = new();

        public event Action<Arcana, int> OnBondRankUp;

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

        public void RegisterBond(CharacterData character)
        {
            if (bonds.ContainsKey(character.arcana)) return;

            bonds[character.arcana] = new BondProgress
            {
                character = character,
                currentRank = 0,
                currentPoints = 0
            };
        }

        public void InitiateBond(Arcana arcana)
        {
            if (!bonds.ContainsKey(arcana)) return;

            var bond = bonds[arcana];
            if (bond.currentRank == 0)
            {
                bond.currentRank = 1;
                OnBondRankUp?.Invoke(arcana, 1);
                GameEventBus.Publish(new BondRankUpEvent(arcana, 1));
            }
        }

        public void AddBondPoints(Arcana arcana, int points)
        {
            if (!bonds.ContainsKey(arcana)) return;

            var bond = bonds[arcana];
            if (bond.isMaxed) return;

            bond.currentPoints += points;

            if (bond.currentPoints >= bond.PointsToNextRank && bond.currentRank < 10)
            {
                bond.currentPoints -= bond.PointsToNextRank;
                bond.currentRank++;

                if (bond.currentRank >= bond.character.maxBondRank)
                    bond.isMaxed = true;

                OnBondRankUp?.Invoke(arcana, bond.currentRank);
                GameEventBus.Publish(new BondRankUpEvent(arcana, bond.currentRank));
            }
        }

        public BondProgress GetBondProgress(Arcana arcana)
        {
            return bonds.TryGetValue(arcana, out var bond) ? bond : null;
        }

        public int GetBondRank(Arcana arcana)
        {
            return bonds.TryGetValue(arcana, out var bond) ? bond.currentRank : 0;
        }

        public int GetTotalBondRanks()
        {
            int total = 0;
            foreach (var bond in bonds.Values)
                total += bond.currentRank;
            return total;
        }

        public bool AreBondsMaxed(params Arcana[] arcanas)
        {
            foreach (var arcana in arcanas)
            {
                if (!bonds.ContainsKey(arcana) || !bonds[arcana].isMaxed)
                    return false;
            }
            return true;
        }
    }

    public readonly struct BondRankUpEvent
    {
        public readonly Arcana Arcana;
        public readonly int NewRank;

        public BondRankUpEvent(Arcana arcana, int newRank)
        {
            Arcana = arcana;
            NewRank = newRank;
        }
    }
}
