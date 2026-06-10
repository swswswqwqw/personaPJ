using System;
using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Data;

namespace ArcadiaOfEchoes.Core
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [SerializeField] private PlayerStatsData config;

        private readonly Dictionary<SocialStatType, int> _statValues = new();

        public event Action<SocialStatType, int, int> OnStatChanged;
        public event Action<SocialStatType, int> OnStatRankUp;

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
            foreach (SocialStatType stat in Enum.GetValues(typeof(SocialStatType)))
            {
                _statValues[stat] = 0;
            }
        }

        public void AddStatPoints(SocialStatType type, int points)
        {
            int oldValue = _statValues[type];
            int oldRank = GetStatRank(type);
            _statValues[type] += points;
            int newRank = GetStatRank(type);

            OnStatChanged?.Invoke(type, oldValue, _statValues[type]);

            if (newRank > oldRank)
                OnStatRankUp?.Invoke(type, newRank);
        }

        public int GetStatValue(SocialStatType type)
        {
            return _statValues.GetValueOrDefault(type, 0);
        }

        public int GetStatRank(SocialStatType type)
        {
            if (config == null) return 0;
            var cfg = config.GetConfig(type);
            if (cfg == null) return 0;

            int value = _statValues.GetValueOrDefault(type, 0);
            for (int i = cfg.ThresholdsPerRank.Length - 1; i >= 0; i--)
            {
                if (value >= cfg.ThresholdsPerRank[i])
                    return i + 1;
            }
            return 1;
        }

        public string GetStatRankName(SocialStatType type)
        {
            if (config == null) return "";
            var cfg = config.GetConfig(type);
            int rank = GetStatRank(type) - 1;
            if (cfg == null || rank < 0 || rank >= cfg.RankNames.Length)
                return "";
            return cfg.RankNames[rank];
        }
    }
}
