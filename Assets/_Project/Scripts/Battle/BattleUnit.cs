using System.Collections.Generic;
using System.Linq;
using AstralEchoes.Data;

namespace AstralEchoes.Battle
{
    public class BattleUnit
    {
        public string Id { get; }
        public string NameJP { get; }
        public bool IsEnemy { get; }
        public CharacterStats Stats { get; }
        public List<AttributeResistance> Resistances { get; }

        public int CurrentHP { get; private set; }
        public int CurrentSP { get; private set; }
        public bool IsAlive => CurrentHP > 0;
        public bool IsDown { get; private set; }
        public bool IsGuarding { get; private set; }
        public StatusAilment CurrentAilment { get; private set; } = StatusAilment.None;

        public int BaseExpReward { get; set; }
        public int BaseMoneyReward { get; set; }

        public BattleUnit(string id, string name, bool isEnemy, CharacterStats stats, List<AttributeResistance> resistances)
        {
            Id = id;
            NameJP = name;
            IsEnemy = isEnemy;
            Stats = stats;
            Resistances = resistances ?? new List<AttributeResistance>();
            CurrentHP = stats.MaxHP;
            CurrentSP = stats.MaxSP;
        }

        public void ApplyDamage(int damage)
        {
            if (IsGuarding) damage /= 2;
            CurrentHP = System.Math.Max(0, CurrentHP - damage);
        }

        public void Heal(int amount)
        {
            CurrentHP = System.Math.Min(Stats.MaxHP, CurrentHP + amount);
        }

        public void SpendSP(int amount)
        {
            CurrentSP = System.Math.Max(0, CurrentSP - amount);
        }

        public void RecoverSP(int amount)
        {
            CurrentSP = System.Math.Min(Stats.MaxSP, CurrentSP + amount);
        }

        public void SetDown(bool down) => IsDown = down;
        public void SetGuard(bool guard) => IsGuarding = guard;
        public void SetAilment(StatusAilment ailment) => CurrentAilment = ailment;

        public void ResetTurnState()
        {
            IsGuarding = false;
            if (IsDown) IsDown = false;
        }

        public ResistanceLevel GetResistance(Attribute attribute)
        {
            var match = Resistances.FirstOrDefault(r => r.Attribute == attribute);
            return match?.Level ?? ResistanceLevel.Normal;
        }
    }
}
