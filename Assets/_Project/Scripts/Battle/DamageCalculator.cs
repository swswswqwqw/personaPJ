using System;
using AstralEchoes.Data;

namespace AstralEchoes.Battle
{
    public static class DamageCalculator
    {
        static readonly Random _rng = new();

        public static DamageResult Calculate(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            var result = new DamageResult
            {
                Attacker = attacker,
                Target = target,
                Skill = skill
            };

            var resistance = target.GetResistance(skill.Attribute);
            result.Resistance = resistance;

            if (resistance == ResistanceLevel.Null)
            {
                result.FinalDamage = 0;
                result.IsNulled = true;
                return result;
            }

            if (resistance == ResistanceLevel.Reflect)
            {
                result.IsReflected = true;
                result.FinalDamage = CalculateBaseDamage(attacker, target, skill);
                return result;
            }

            if (resistance == ResistanceLevel.Absorb)
            {
                result.IsAbsorbed = true;
                result.FinalDamage = CalculateBaseDamage(attacker, target, skill);
                return result;
            }

            int baseDamage = CalculateBaseDamage(attacker, target, skill);

            float multiplier = resistance switch
            {
                ResistanceLevel.Weak => 1.5f,
                ResistanceLevel.Resist => 0.5f,
                _ => 1.0f
            };

            result.IsWeakness = resistance == ResistanceLevel.Weak;

            float critRoll = (float)_rng.NextDouble();
            result.IsCritical = critRoll < skill.CritRate + attacker.Stats.Luck * 0.005f;
            if (result.IsCritical) multiplier *= 1.5f;

            float variance = 0.95f + (float)_rng.NextDouble() * 0.1f;

            result.FinalDamage = Math.Max(1, (int)(baseDamage * multiplier * variance));
            return result;
        }

        static int CalculateBaseDamage(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            bool isPhysical = skill.Attribute is Attribute.Slash or Attribute.Strike or Attribute.Pierce;

            int attackStat = isPhysical ? attacker.Stats.Strength : attacker.Stats.Magic;
            int defenseStat = isPhysical ? target.Stats.Endurance : target.Stats.Endurance / 2 + target.Stats.Luck / 4;

            int raw = (int)(Math.Sqrt(attackStat * 2.0) * skill.BasePower / 5.0) - defenseStat / 4;
            return Math.Max(1, raw);
        }
    }

    public class DamageResult
    {
        public BattleUnit Attacker;
        public BattleUnit Target;
        public SkillData Skill;
        public int FinalDamage;
        public bool IsWeakness;
        public bool IsCritical;
        public bool IsNulled;
        public bool IsReflected;
        public bool IsAbsorbed;
        public bool IsFullResonance;
        public ResistanceLevel Resistance;
    }
}
