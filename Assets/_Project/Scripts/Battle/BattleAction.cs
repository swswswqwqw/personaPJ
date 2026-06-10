using System.Collections.Generic;

namespace Amane.Battle
{
    public enum ActionType
    {
        Skill,
        Guard,
        Item,
        Flee,
        KotsugiPass  // 言継ぎ（バトンタッチ）
    }

    public sealed class BattleAction
    {
        public ActionType Type { get; }
        public Combatant Actor { get; }
        public Skill Skill { get; }
        public List<Combatant> Targets { get; }

        private BattleAction(ActionType type, Combatant actor, Skill skill, List<Combatant> targets)
        {
            Type = type;
            Actor = actor;
            Skill = skill;
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
    }
}
