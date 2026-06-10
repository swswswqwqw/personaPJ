using System;

namespace Amane.Battle
{
    public static class DamageCalculator
    {
        private static readonly Random Rng = new();

        public static HitResult Calculate(Combatant attacker, Combatant target, Skill skill,
                                          float kotsugiBonus = 0f)
        {
            var affinity = target.Affinities.Get(skill.Element);

            switch (affinity)
            {
                case Affinity.Null:
                    return new HitResult(target, 0, HitType.Null, false);
                case Affinity.Repel:
                    int repelDmg = ComputeRaw(attacker, attacker, skill);
                    attacker.TakeDamage(repelDmg);
                    return new HitResult(target, 0, HitType.Repel, false);
                case Affinity.Drain:
                    int drainAmt = ComputeRaw(attacker, target, skill) / 2;
                    target.Heal(drainAmt);
                    return new HitResult(target, -drainAmt, HitType.Drain, false);
            }

            int raw = ComputeRaw(attacker, target, skill);

            bool isCrit = !skill.IsPhysical ? false : Rng.NextDouble() < skill.CritRate;

            float multiplier = 1f + kotsugiBonus;
            if (affinity == Affinity.Weak) multiplier *= 1.5f;
            if (affinity == Affinity.Resist) multiplier *= 0.5f;
            if (isCrit) multiplier *= 1.5f;

            int finalDmg = Math.Max(1, (int)(raw * multiplier));
            int dealt = target.TakeDamage(finalDmg);

            var hitType = affinity == Affinity.Weak ? HitType.Weak
                        : isCrit ? HitType.Critical
                        : affinity == Affinity.Resist ? HitType.Resist
                        : HitType.Normal;

            bool downed = false;
            if ((hitType == HitType.Weak || hitType == HitType.Critical) && target.IsAlive && !target.IsDown)
            {
                target.SetDown(true);
                downed = true;
            }

            return new HitResult(target, dealt, hitType, downed);
        }

        private static int ComputeRaw(Combatant atk, Combatant def, Skill skill)
        {
            int offense = skill.IsPhysical ? atk.Attack : atk.MagicAttack;
            int defense = skill.IsPhysical ? def.Defense : def.MagicDefense;
            float variation = 0.9f + (float)Rng.NextDouble() * 0.2f;
            return Math.Max(1, (int)((skill.BasePower * (offense / (float)(defense + 1)) * 0.5f + 5) * variation));
        }
    }
}
