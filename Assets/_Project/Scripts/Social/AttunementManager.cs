using System;
using System.Collections.Generic;
using System.Linq;
using AstralEchoes.Core;
using AstralEchoes.Data;

namespace AstralEchoes.Social
{
    public sealed class AttunementManager
    {
        static AttunementManager _instance;
        public static AttunementManager Instance => _instance ??= new AttunementManager();

        readonly Dictionary<string, AttunementData> _attunements = new();

        public event Action<string, int> OnRankUp;

        AttunementManager() { }

        public void RegisterAttunement(string characterId, Arcana arcana)
        {
            if (_attunements.ContainsKey(characterId)) return;
            _attunements[characterId] = new AttunementData
            {
                CharacterId = characterId,
                Arcana = arcana,
                Rank = 0,
                Points = 0,
                IsAvailable = false
            };
        }

        public void UnlockAttunement(string characterId)
        {
            if (_attunements.TryGetValue(characterId, out var data))
                data.IsAvailable = true;
        }

        public bool TryAddPoints(string characterId, int points)
        {
            if (!_attunements.TryGetValue(characterId, out var data)) return false;
            if (!data.IsAvailable || data.Rank >= 10) return false;

            data.Points += points;

            int threshold = GetPointThreshold(data.Rank);
            if (data.Points >= threshold && data.Rank < 10)
            {
                data.Rank++;
                data.Points = 0;

                OnRankUp?.Invoke(characterId, data.Rank);
                EventBus.Publish(new AttunementRankUpEvent
                {
                    CharacterId = characterId,
                    CharacterArcana = data.Arcana,
                    NewRank = data.Rank
                });
            }

            return true;
        }

        public int GetRank(string characterId)
        {
            return _attunements.TryGetValue(characterId, out var data) ? data.Rank : 0;
        }

        public int GetPoints(string characterId)
        {
            return _attunements.TryGetValue(characterId, out var data) ? data.Points : 0;
        }

        public bool IsAvailable(string characterId)
        {
            return _attunements.TryGetValue(characterId, out var data) && data.IsAvailable;
        }

        public int GetTotalAttunement()
        {
            return _attunements.Values.Sum(a => a.Rank);
        }

        public bool MeetsEndingRequirement()
        {
            var partyMembers = _attunements.Values.Where(a => a.IsPartyMember);
            return partyMembers.All(a => a.Rank >= 8) && GetTotalAttunement() >= 70;
        }

        static int GetPointThreshold(int currentRank)
        {
            return currentRank switch
            {
                0 => 0,
                1 => 5,
                2 => 10,
                3 => 15,
                4 => 15,
                5 => 20,
                6 => 22,
                7 => 25,
                8 => 30,
                9 => 35,
                _ => 999
            };
        }

        class AttunementData
        {
            public string CharacterId;
            public Arcana Arcana;
            public int Rank;
            public int Points;
            public bool IsAvailable;
            public bool IsPartyMember;
        }
    }
}
