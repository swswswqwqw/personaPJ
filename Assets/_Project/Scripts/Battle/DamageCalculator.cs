using UnityEngine;
using Astra.Data;

namespace Astra.Battle
{
    public struct AttackResult
    {
        public int Damage;
        public bool HitWeakness;
        public bool IsCritical;
        public bool WasResisted;
        public bool WasNulled;
        public bool WasAbsorbed;
        public bool WasRepelled;
        public bool Missed;
    }

    public static class DamageCalculator
    {
        public static AttackResult Calculate(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            var result = new AttackResult();

            float hitRoll = Random.value;
            if (hitRoll > skill.accuracy)
            {
                result.Missed = true;
                return result;
            }

            var affinity = target.GetAffinity(skill.element);

            switch (affinity)
            {
                case AffinityType.Null:
                    result.WasNulled = true;
                    result.Damage = 0;
                    return result;
                case AffinityType.Absorb:
                    result.WasAbsorbed = true;
                    result.Damage = CalculateRawDamage(attacker, target, skill);
                    target.Heal(result.Damage);
                    return result;
                case AffinityType.Repel:
                    result.WasRepelled = true;
                    result.Damage = CalculateRawDamage(attacker, target, skill);
                    attacker.TakeDamage(result.Damage);
                    return result;
            }

            int rawDamage = CalculateRawDamage(attacker, target, skill);

            float affinityMultiplier = affinity switch
            {
                AffinityType.Weak => 1.5f,
                AffinityType.Resist => 0.5f,
                _ => 1.0f
            };

            result.HitWeakness = affinity == AffinityType.Weak;
            result.WasResisted = affinity == AffinityType.Resist;

            int critRoll = Random.Range(0, 100);
            result.IsCritical = critRoll < skill.criticalRate + attacker.Luck / 2;
            float critMultiplier = result.IsCritical ? 1.5f : 1.0f;

            float variance = Random.Range(0.9f, 1.1f);

            result.Damage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * affinityMultiplier * critMultiplier * variance));

            return result;
        }

        private static int CalculateRawDamage(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            bool isPhysical = skill.element == ElementType.Physical;
            int attackStat = isPhysical ? attacker.Strength : attacker.Magic;
            int defenseStat = isPhysical ? target.Endurance : target.Endurance / 2 + target.Luck / 4;

            return Mathf.Max(1, skill.power * attackStat / Mathf.Max(1, defenseStat));
        }
    }
}
