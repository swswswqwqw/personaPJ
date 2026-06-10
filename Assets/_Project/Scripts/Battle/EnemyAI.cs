using System;
using System.Collections.Generic;
using System.Linq;

namespace Amane.Battle
{
    public static class EnemyAI
    {
        private static readonly Random Rng = new();

        public static BattleAction DecideAction(Combatant enemy, List<Combatant> party)
        {
            var aliveTargets = party.Where(p => p.IsAlive).ToList();
            if (aliveTargets.Count == 0) return BattleAction.Guard(enemy);

            var usableSkills = enemy.Skills.Where(s => enemy.Sp >= s.SpCost).ToList();

            foreach (var skill in usableSkills)
            {
                foreach (var target in aliveTargets)
                {
                    if (target.Affinities.Get(skill.Element) == Affinity.Weak)
                    {
                        var targets = skill.Target == TargetType.AllEnemies
                            ? aliveTargets
                            : new List<Combatant> { target };
                        return BattleAction.UseSkill(enemy, skill, targets);
                    }
                }
            }

            if (usableSkills.Count > 0)
            {
                var skill = usableSkills[Rng.Next(usableSkills.Count)];
                var targets = skill.Target == TargetType.AllEnemies
                    ? aliveTargets
                    : new List<Combatant> { aliveTargets[Rng.Next(aliveTargets.Count)] };
                return BattleAction.UseSkill(enemy, skill, targets);
            }

            var meleeTarget = aliveTargets[Rng.Next(aliveTargets.Count)];
            return BattleAction.UseSkill(enemy, Skill.MeleeAttack, new List<Combatant> { meleeTarget });
        }
    }
}
