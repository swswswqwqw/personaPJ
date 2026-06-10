using UnityEngine;
using Astra.Data;

namespace Astra.Battle
{
    public class BattleUnit
    {
        public string Name { get; }
        public int MaxHP { get; }
        public int MaxSP { get; }
        public int CurrentHP { get; private set; }
        public int CurrentSP { get; private set; }
        public int Strength { get; }
        public int Magic { get; }
        public int Endurance { get; }
        public int Agility { get; }
        public int Luck { get; }

        public bool IsDead => CurrentHP <= 0;
        public bool IsPlayer { get; }

        private readonly CharacterData _characterData;
        private readonly EnemyData _enemyData;

        public BattleUnit(CharacterData data)
        {
            IsPlayer = true;
            _characterData = data;
            Name = data.characterName;
            MaxHP = data.maxHP;
            MaxSP = data.maxSP;
            CurrentHP = MaxHP;
            CurrentSP = MaxSP;
            Strength = data.strength;
            Magic = data.magic;
            Endurance = data.endurance;
            Agility = data.agility;
            Luck = data.luck;
        }

        public BattleUnit(EnemyData data)
        {
            IsPlayer = false;
            _enemyData = data;
            Name = data.enemyName;
            MaxHP = data.maxHP;
            MaxSP = data.maxSP;
            CurrentHP = MaxHP;
            CurrentSP = MaxSP;
            Strength = data.strength;
            Magic = data.magic;
            Endurance = data.endurance;
            Agility = data.agility;
            Luck = data.luck;
        }

        public AffinityType GetAffinity(ElementType element)
        {
            if (_characterData != null) return _characterData.GetAffinity(element);
            if (_enemyData != null) return _enemyData.GetAffinity(element);
            return AffinityType.Normal;
        }

        public void TakeDamage(int damage)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - damage);
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        }

        public void ConsumeSP(int amount)
        {
            CurrentSP = Mathf.Max(0, CurrentSP - amount);
        }

        public void RestoreSP(int amount)
        {
            CurrentSP = Mathf.Min(MaxSP, CurrentSP + amount);
        }
    }
}
