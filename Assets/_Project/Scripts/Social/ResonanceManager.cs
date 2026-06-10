using System;
using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Core;
using ArcadiaOfEchoes.Data;

namespace ArcadiaOfEchoes.Social
{
    public class ResonanceManager : MonoBehaviour
    {
        public static ResonanceManager Instance { get; private set; }

        [SerializeField] private List<CharacterData> resonanceCharacters;

        private readonly Dictionary<string, int> _resonanceRanks = new();
        private readonly Dictionary<string, int> _resonancePoints = new();

        public event Action<string, int> OnResonanceRankUp;
        public event Action<string, int> OnResonancePointsGained;

        private static readonly int[] PointsPerRank = { 0, 2, 4, 7, 10, 15, 20, 27, 35, 45 };

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

        public void Initialize()
        {
            foreach (var character in resonanceCharacters)
            {
                _resonanceRanks[character.CharacterId] = 0;
                _resonancePoints[character.CharacterId] = 0;
            }
        }

        public void AddResonancePoints(string characterId, int points)
        {
            if (!_resonanceRanks.ContainsKey(characterId)) return;
            if (_resonanceRanks[characterId] >= 10) return;

            _resonancePoints[characterId] += points;
            OnResonancePointsGained?.Invoke(characterId, points);

            CheckRankUp(characterId);
        }

        public int GetResonanceRank(string characterId)
        {
            return _resonanceRanks.GetValueOrDefault(characterId, 0);
        }

        public float GetResonanceDamageMultiplier(string characterId)
        {
            int rank = GetResonanceRank(characterId);
            return 1.0f + (rank * 0.05f);
        }

        public bool IsWallReached(string characterId)
        {
            return GetResonanceRank(characterId) == 5;
        }

        public bool IsMaxRank(string characterId)
        {
            return GetResonanceRank(characterId) >= 10;
        }

        private void CheckRankUp(string characterId)
        {
            int currentRank = _resonanceRanks[characterId];
            if (currentRank >= 10) return;

            int pointsNeeded = PointsPerRank[currentRank];
            if (_resonancePoints[characterId] >= pointsNeeded)
            {
                _resonancePoints[characterId] -= pointsNeeded;
                _resonanceRanks[characterId]++;
                int newRank = _resonanceRanks[characterId];

                OnResonanceRankUp?.Invoke(characterId, newRank);
                EventBus.Publish(new ResonanceRankUpEvent
                {
                    CharacterId = characterId,
                    NewRank = newRank
                });
            }
        }
    }
}
