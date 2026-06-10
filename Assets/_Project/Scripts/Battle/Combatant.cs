using System;
using System.Collections.Generic;

namespace Amane.Battle
{
    public sealed class Combatant
    {
        public string Id { get; }
        public string DisplayName { get; }
        public bool IsPlayer { get; }

        public int MaxHp { get; }
        public int MaxSp { get; }
        public int Hp { get; private set; }
        public int Sp { get; private set; }

        public int Attack { get; }
        public int Defense { get; }
        public int MagicAttack { get; }
        public int MagicDefense { get; }
        public int Agility { get; }

        public AffinityTable Affinities { get; }
        public List<Skill> Skills { get; }

        public bool IsDown { get; private set; }
        public bool IsAlive => Hp > 0;

        public Combatant(string id, string displayName, bool isPlayer,
                         int maxHp, int maxSp, int atk, int def,
                         int matk, int mdef, int agi,
                         AffinityTable affinities, List<Skill> skills = null)
        {
            Id = id;
            DisplayName = displayName;
            IsPlayer = isPlayer;
            MaxHp = maxHp;
            MaxSp = maxSp;
            Hp = maxHp;
            Sp = maxSp;
            Attack = atk;
            Defense = def;
            MagicAttack = matk;
            MagicDefense = mdef;
            Agility = agi;
            Affinities = affinities ?? AffinityTable.AllNormal();
            Skills = skills ?? new List<Skill>();
        }

        public int TakeDamage(int amount)
        {
            int clamped = Math.Max(0, amount);
            Hp = Math.Max(0, Hp - clamped);
            return clamped;
        }

        public void Heal(int amount) => Hp = Math.Min(MaxHp, Hp + Math.Max(0, amount));

        public bool SpendSp(int cost)
        {
            if (Sp < cost) return false;
            Sp -= cost;
            return true;
        }

        public void RestoreSp(int amount) => Sp = Math.Min(MaxSp, Sp + Math.Max(0, amount));

        public void SetDown(bool down) => IsDown = down;

        public void Revive(int hpPercent = 50)
        {
            if (IsAlive) return;
            Hp = Math.Max(1, MaxHp * hpPercent / 100);
            IsDown = false;
        }
    }
}
