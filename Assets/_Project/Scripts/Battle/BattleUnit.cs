using System.Collections.Generic;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Battle
{
    public class BattleUnit
    {
        public string Name { get; }
        public int MaxHP { get; }
        public int MaxSP { get; }
        public int CurrentHP { get; set; }
        public int CurrentSP { get; set; }
        public int Strength { get; }
        public int Magic { get; }
        public int Endurance { get; }
        public int Agility { get; }
        public int Luck { get; }
        public bool IsDown { get; set; }
        public bool IsAlive => CurrentHP > 0;

        public Dictionary<ElementType, Affinity> Affinities { get; } = new();
        public List<AbilityData> Abilities { get; } = new();

        public BattleUnit(CharacterData data)
        {
            Name = data.characterName;
            MaxHP = data.baseHP;
            MaxSP = data.baseSP;
            CurrentHP = MaxHP;
            CurrentSP = MaxSP;
            Strength = data.baseStrength;
            Magic = data.baseMagic;
            Endurance = data.baseEndurance;
            Agility = data.baseAgility;
            Luck = data.baseLuck;

            foreach (var entry in data.affinities)
                Affinities[entry.element] = entry.affinity;
        }

        public BattleUnit(EnemyData data)
        {
            Name = data.enemyName;
            MaxHP = data.hp;
            MaxSP = data.sp;
            CurrentHP = MaxHP;
            CurrentSP = MaxSP;
            Strength = data.strength;
            Magic = data.magic;
            Endurance = data.endurance;
            Agility = data.agility;
            Luck = data.luck;

            foreach (var entry in data.affinities)
                Affinities[entry.element] = entry.affinity;
        }

        public Affinity GetAffinity(ElementType element)
        {
            return Affinities.TryGetValue(element, out var aff) ? aff : Affinity.Normal;
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;
            CurrentHP = System.Math.Max(0, CurrentHP - damage);
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;
            CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
        }

        public void RestoreSP(int amount)
        {
            if (!IsAlive) return;
            CurrentSP = System.Math.Min(MaxSP, CurrentSP + amount);
        }

        public void Revive(int hpAmount)
        {
            if (IsAlive) return;
            CurrentHP = System.Math.Min(MaxHP, System.Math.Max(1, hpAmount));
            IsDown = false;
        }
    }
}
