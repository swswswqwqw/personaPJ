using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Battle
{
    public struct EnemyAction
    {
        public AbilityData ability;
        public BattleUnit target;
    }

    public static class EnemyAI
    {
        public static EnemyAction DecideAction(BattleUnit enemy, BattleManager battleManager)
        {
            if (battleManager == null)
                return new EnemyAction { ability = null, target = null };

            var partyTargets = battleManager.PartyUnits;

            if (enemy.Abilities.Count == 0)
                return CreateBasicAttack(enemy, GetRandomAliveTarget(partyTargets));

            var usableAbilities = new List<AbilityData>();
            foreach (var ability in enemy.Abilities)
            {
                if (enemy.CurrentSP >= ability.spCost)
                    usableAbilities.Add(ability);
            }

            if (usableAbilities.Count == 0)
                return CreateBasicAttack(enemy, GetRandomAliveTarget(partyTargets));

            // 戦略: 弱点を突ける技を優先する
            foreach (var ability in usableAbilities)
            {
                if (ability.element == ElementType.Heal || ability.element == ElementType.Strike)
                    continue;

                foreach (var target in partyTargets)
                {
                    if (!target.IsAlive) continue;
                    if (target.GetAffinity(ability.element) == Affinity.Weak)
                    {
                        return new EnemyAction { ability = ability, target = target };
                    }
                }
            }

            // 弱点がない場合: HP低い味方を回復するか、ランダム攻撃
            if (ShouldHeal(enemy))
            {
                foreach (var ability in usableAbilities)
                {
                    if (ability.element == ElementType.Heal)
                        return new EnemyAction { ability = ability, target = enemy };
                }
            }

            // ランダムに使用可能な攻撃スキルを選択
            var attackAbilities = new List<AbilityData>();
            foreach (var ability in usableAbilities)
            {
                if (ability.element != ElementType.Heal)
                    attackAbilities.Add(ability);
            }

            if (attackAbilities.Count > 0)
            {
                var chosen = attackAbilities[Random.Range(0, attackAbilities.Count)];
                BattleUnit target;

                if (chosen.targetType is TargetType.AllEnemies or TargetType.AllAllies)
                    target = GetRandomAliveTarget(partyTargets);
                else
                    target = GetWeakestAliveTarget(partyTargets);

                return new EnemyAction { ability = chosen, target = target };
            }

            return CreateBasicAttack(enemy, GetRandomAliveTarget(partyTargets));
        }

        private static EnemyAction CreateBasicAttack(BattleUnit enemy, BattleUnit target)
        {
            var attack = ScriptableObject.CreateInstance<AbilityData>();
            attack.abilityName = "攻撃";
            attack.element = ElementType.Strike;
            attack.targetType = TargetType.SingleEnemy;
            attack.basePower = 25;
            attack.spCost = 0;
            attack.accuracy = 0.9f;
            attack.criticalRate = 5;

            return new EnemyAction { ability = attack, target = target };
        }

        private static bool ShouldHeal(BattleUnit enemy)
        {
            return (float)enemy.CurrentHP / enemy.MaxHP < 0.3f;
        }

        private static BattleUnit GetRandomAliveTarget(List<BattleUnit> units)
        {
            var alive = new List<BattleUnit>();
            foreach (var unit in units)
                if (unit.IsAlive) alive.Add(unit);

            return alive.Count > 0 ? alive[Random.Range(0, alive.Count)] : units[0];
        }

        private static BattleUnit GetWeakestAliveTarget(List<BattleUnit> units)
        {
            BattleUnit weakest = null;
            float lowestRatio = float.MaxValue;

            foreach (var unit in units)
            {
                if (!unit.IsAlive) continue;
                float ratio = (float)unit.CurrentHP / unit.MaxHP;
                if (ratio < lowestRatio)
                {
                    lowestRatio = ratio;
                    weakest = unit;
                }
            }

            return weakest ?? GetRandomAliveTarget(units);
        }
    }
}
