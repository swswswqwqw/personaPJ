using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Social
{
    [Serializable]
    public class ResonanceLink
    {
        public CharacterData Character;
        public int CurrentRank;
        public int CurrentPoints;
        public bool IsReversed;
        public bool IsBroken;
        public List<int> CompletedEvents = new();

        public int PointsForNextRank => CurrentRank switch
        {
            0 => 0,
            1 => 15,
            2 => 20,
            3 => 25,
            4 => 30,
            5 => 35,
            6 => 40,
            7 => 50,
            8 => 55,
            9 => 60,
            _ => int.MaxValue
        };

        public bool CanRankUp => CurrentRank < 10 && CurrentPoints >= PointsForNextRank;
    }

    public class ResonanceManager : MonoBehaviour
    {
        public static ResonanceManager Instance { get; private set; }

        private Dictionary<Arcana, ResonanceLink> _links = new();

        public event Action<Arcana, int> OnRankUp;
        public event Action<Arcana, int> OnPointsGained;
        public event Action<Arcana> OnReversed;

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

        public void InitializeLink(CharacterData character)
        {
            if (_links.ContainsKey(character.arcana)) return;

            _links[character.arcana] = new ResonanceLink
            {
                Character = character,
                CurrentRank = 0,
                CurrentPoints = 0
            };
        }

        public void AddPoints(Arcana arcana, int points)
        {
            if (!_links.ContainsKey(arcana)) return;
            var link = _links[arcana];

            if (link.CurrentRank >= 10 || link.IsBroken) return;

            link.CurrentPoints += points;
            OnPointsGained?.Invoke(arcana, points);
            GameEventBus.Publish(new ResonancePointsGainedEvent(arcana, points, link.CurrentPoints));
        }

        public bool TryRankUp(Arcana arcana)
        {
            if (!_links.ContainsKey(arcana)) return false;
            var link = _links[arcana];

            if (!link.CanRankUp) return false;

            link.CurrentPoints -= link.PointsForNextRank;
            link.CurrentRank++;

            OnRankUp?.Invoke(arcana, link.CurrentRank);
            GameEventBus.Publish(new ResonanceRankUpEvent(arcana, link.CurrentRank));

            return true;
        }

        public ResonanceLink GetLink(Arcana arcana)
        {
            return _links.TryGetValue(arcana, out var link) ? link : null;
        }

        public int GetRank(Arcana arcana)
        {
            return _links.TryGetValue(arcana, out var link) ? link.CurrentRank : 0;
        }

        public IReadOnlyDictionary<Arcana, ResonanceLink> GetAllLinks() => _links;

        public float GetResonanceDamageMultiplier(Arcana arcana)
        {
            int rank = GetRank(arcana);
            return 1.0f + rank * 0.05f;
        }

        public void ReverseLink(Arcana arcana)
        {
            if (!_links.ContainsKey(arcana)) return;
            _links[arcana].IsReversed = true;
            OnReversed?.Invoke(arcana);
        }

        public void RepairLink(Arcana arcana)
        {
            if (!_links.ContainsKey(arcana)) return;
            _links[arcana].IsReversed = false;
        }
    }

    public readonly struct ResonanceRankUpEvent
    {
        public readonly Arcana Arcana;
        public readonly int NewRank;
        public ResonanceRankUpEvent(Arcana arcana, int rank)
        {
            Arcana = arcana;
            NewRank = rank;
        }
    }

    public readonly struct ResonancePointsGainedEvent
    {
        public readonly Arcana Arcana;
        public readonly int PointsGained;
        public readonly int TotalPoints;
        public ResonancePointsGainedEvent(Arcana arcana, int gained, int total)
        {
            Arcana = arcana;
            PointsGained = gained;
            TotalPoints = total;
        }
    }
}
