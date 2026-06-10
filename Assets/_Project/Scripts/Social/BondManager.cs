using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfEchoes.Core;
using AriaOfEchoes.Data;

namespace AriaOfEchoes.Social
{
    [Serializable]
    public class BondData
    {
        public CharacterData character;
        public int currentRank;
        public int currentPoints;
        public bool isUnlocked;
        public bool isReversed;

        public int PointsToNextRank => (currentRank + 1) * 100;
        public bool IsMaxRank => currentRank >= 10;
    }

    public class BondManager : MonoBehaviour
    {
        public static BondManager Instance { get; private set; }

        Dictionary<string, BondData> bonds = new();

        public event Action<BondData> OnBondRankUp;
        public event Action<BondData> OnBondPointsChanged;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterBond(CharacterData character)
        {
            if (bonds.ContainsKey(character.characterName)) return;

            bonds[character.characterName] = new BondData
            {
                character = character,
                currentRank = 0,
                currentPoints = 0,
                isUnlocked = false,
                isReversed = false
            };
        }

        public void UnlockBond(CharacterData character)
        {
            if (!bonds.TryGetValue(character.characterName, out var bond)) return;

            bond.isUnlocked = true;
            bond.currentRank = 1;
            EventBus.Publish(new BondUnlockedEvent(character));
        }

        public void AddBondPoints(CharacterData character, int points)
        {
            if (!bonds.TryGetValue(character.characterName, out var bond)) return;
            if (!bond.isUnlocked || bond.IsMaxRank) return;

            bond.currentPoints += points;
            OnBondPointsChanged?.Invoke(bond);

            while (bond.currentPoints >= bond.PointsToNextRank && !bond.IsMaxRank)
            {
                bond.currentPoints -= bond.PointsToNextRank;
                bond.currentRank++;
                OnBondRankUp?.Invoke(bond);
                EventBus.Publish(new BondRankUpEvent(character, bond.currentRank));
            }
        }

        public BondData GetBond(CharacterData character)
        {
            bonds.TryGetValue(character.characterName, out var bond);
            return bond;
        }

        public int GetBondRank(CharacterData character)
        {
            if (bonds.TryGetValue(character.characterName, out var bond))
                return bond.currentRank;
            return 0;
        }

        public List<BondData> GetAllBonds()
        {
            return new List<BondData>(bonds.Values);
        }

        public List<BondData> GetUnlockedBonds()
        {
            var result = new List<BondData>();
            foreach (var bond in bonds.Values)
            {
                if (bond.isUnlocked)
                    result.Add(bond);
            }
            return result;
        }
    }

    public struct BondUnlockedEvent
    {
        public CharacterData Character;
        public BondUnlockedEvent(CharacterData c) { Character = c; }
    }

    public struct BondRankUpEvent
    {
        public CharacterData Character;
        public int NewRank;
        public BondRankUpEvent(CharacterData c, int rank)
        { Character = c; NewRank = rank; }
    }
}
