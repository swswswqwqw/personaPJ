using System.Collections.Generic;

namespace Amane.Battle
{
    public enum ActionType
    {
        Skill,
        Guard,
        Item,
        Flee,
        KotsugiPass,  // 言継ぎ（バトンタッチ）
        DualNarrator  // DESIGN.md 9-1: デュアルナレーター（1ターン2スキル — 第3幕解放）
    }

    public sealed class BattleAction
    {
        public ActionType Type { get; }
        public Combatant Actor { get; }
        public Skill Skill { get; }
        // DualNarrator時のサブスキル（Secondaryナレーターのスキル）
        public Skill SecondarySkill { get; }
        public List<Combatant> Targets { get; }

        private BattleAction(ActionType type, Combatant actor, Skill skill,
                             List<Combatant> targets, Skill secondarySkill = null)
        {
            Type = type;
            Actor = actor;
            Skill = skill;
            SecondarySkill = secondarySkill;
            Targets = targets ?? new List<Combatant>();
        }

        public static BattleAction UseSkill(Combatant actor, Skill skill, List<Combatant> targets)
            => new(ActionType.Skill, actor, skill, targets);

        public static BattleAction Guard(Combatant actor)
            => new(ActionType.Guard, actor, null, null);

        public static BattleAction Flee(Combatant actor)
            => new(ActionType.Flee, actor, null, null);

        public static BattleAction Kotsugi(Combatant from, Combatant to)
            => new(ActionType.KotsugiPass, from, null, new List<Combatant> { to });

        // DESIGN.md 9-1: デュアルナレーターアクション（主スキル＋副スキルを1ターンで使用）
        public static BattleAction DualNarratorAttack(Combatant actor, Skill primary, Skill secondary,
                                                       List<Combatant> targets)
            => new(ActionType.DualNarrator, actor, primary, targets, secondary);
    }
}
