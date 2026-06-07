using UnityEngine;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Battle
{
    public struct AttackResult
    {
        public int Damage;
        public bool IsCritical;
        public bool HitWeakness;
        public bool IsResisted;
        public bool IsNulled;
        public bool IsAbsorbed;
        public bool IsReflected;
        public bool IsMiss;
    }

    public static class DamageCalculator
    {
        public static AttackResult Calculate(BattleUnit attacker, BattleUnit target, AbilityData ability)
        {
            var result = new AttackResult();

            float hitRoll = Random.value;
            if (hitRoll > ability.accuracy)
            {
                result.IsMiss = true;
                return result;
            }

            var affinity = target.GetAffinity(ability.element);

            switch (affinity)
            {
                case Affinity.Null:
                    result.IsNulled = true;
                    return result;

                case Affinity.Absorb:
                    result.IsAbsorbed = true;
                    result.Damage = -CalculateBaseDamage(attacker, target, ability);
                    return result;

                case Affinity.Reflect:
                    result.IsReflected = true;
                    result.Damage = CalculateBaseDamage(attacker, target, ability);
                    return result;
            }

            int baseDamage = CalculateBaseDamage(attacker, target, ability);

            if (affinity == Affinity.Weak)
            {
                result.HitWeakness = true;
                baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);
            }
            else if (affinity == Affinity.Resist)
            {
                result.IsResisted = true;
                baseDamage = Mathf.RoundToInt(baseDamage * 0.5f);
            }

            int critRoll = Random.Range(0, 100);
            if (critRoll < ability.criticalRate + attacker.Luck / 5)
            {
                result.IsCritical = true;
                baseDamage = Mathf.RoundToInt(baseDamage * 1.5f);

                if (!result.HitWeakness)
                    result.HitWeakness = true;
            }

            float variance = Random.Range(0.9f, 1.1f);
            result.Damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * variance));

            return result;
        }

        private static int CalculateBaseDamage(BattleUnit attacker, BattleUnit target, AbilityData ability)
        {
            bool isPhysical = ability.element == ElementType.Strike;
            int attackStat = isPhysical ? attacker.Strength : attacker.Magic;
            int defenseStat = isPhysical ? target.Endurance : target.Endurance / 2;

            int damage = ability.basePower + attackStat * 2 - defenseStat;
            return Mathf.Max(1, damage);
        }
    }
}
