using AriaOfEchoes.Data;
using UnityEngine;

namespace AriaOfEchoes.Battle
{
    public struct DamageResult
    {
        public int damage;
        public bool isCritical;
        public AffinityType affinityResult;
        public bool isMiss;
    }

    public static class DamageCalculator
    {
        const float BASE_DAMAGE_FACTOR = 5f;
        const float WEAKNESS_MULTIPLIER = 1.5f;
        const float RESIST_MULTIPLIER = 0.5f;
        const float CRITICAL_MULTIPLIER = 1.5f;
        const float VARIANCE = 0.1f;

        public static DamageResult Calculate(BattleUnit attacker, SkillData skill, BattleUnit target)
        {
            var result = new DamageResult();

            if (Random.value > skill.accuracy)
            {
                result.isMiss = true;
                return result;
            }

            result.affinityResult = target.GetAffinity(skill.element);

            if (result.affinityResult == AffinityType.Null)
            {
                result.damage = 0;
                return result;
            }

            if (result.affinityResult == AffinityType.Absorb)
            {
                result.damage = -(int)CalculateBaseDamage(attacker, skill, target);
                return result;
            }

            float baseDamage = CalculateBaseDamage(attacker, skill, target);

            float affinityMod = result.affinityResult switch
            {
                AffinityType.Weak => WEAKNESS_MULTIPLIER,
                AffinityType.Resist => RESIST_MULTIPLIER,
                _ => 1f
            };

            result.isCritical = Random.value < skill.criticalRate
                + attacker.Stats.Luck * 0.005f;
            float critMod = result.isCritical ? CRITICAL_MULTIPLIER : 1f;

            float variance = 1f + Random.Range(-VARIANCE, VARIANCE);

            result.damage = Mathf.Max(1,
                Mathf.RoundToInt(baseDamage * affinityMod * critMod * variance));

            if (result.affinityResult == AffinityType.Reflect)
                result.damage = Mathf.RoundToInt(baseDamage * variance);

            return result;
        }

        static float CalculateBaseDamage(BattleUnit attacker, SkillData skill, BattleUnit target)
        {
            float attackStat = skill.IsPhysical
                ? attacker.Stats.Strength
                : attacker.Stats.Magic;

            float defenseStat = skill.IsPhysical
                ? target.Stats.Endurance
                : target.Stats.Endurance * 0.7f;

            return BASE_DAMAGE_FACTOR
                * (skill.basePower / 100f)
                * (attackStat / Mathf.Max(1f, defenseStat))
                * skill.hitCount;
        }
    }
}
