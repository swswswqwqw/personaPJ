using System.Collections.Generic;
using UnityEngine;
using AriaOfBacklight.Core;
using AriaOfBacklight.Data;

namespace AriaOfBacklight.Battle
{
    public enum BattleState
    {
        Init,
        PlayerTurn,
        EnemyTurn,
        ResonanceTurn,
        CatharsisBreak,
        Victory,
        Defeat,
        Escape
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public BattleState State { get; private set; }
        public List<BattleUnit> PlayerParty { get; } = new();
        public List<BattleUnit> EnemyParty { get; } = new();
        public BattleUnit ActiveUnit { get; private set; }

        private int turnIndex;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartBattle(List<BattleUnit> players, List<BattleUnit> enemies)
        {
            PlayerParty.Clear();
            PlayerParty.AddRange(players);
            EnemyParty.Clear();
            EnemyParty.AddRange(enemies);

            State = BattleState.Init;
            turnIndex = 0;

            EventBus.Publish(new BattleStartedEvent());
            BeginPlayerTurn();
        }

        private void BeginPlayerTurn()
        {
            if (CheckVictory()) return;

            State = BattleState.PlayerTurn;
            ActiveUnit = GetNextAliveUnit(PlayerParty, ref turnIndex);
            EventBus.Publish(new TurnStartedEvent(ActiveUnit, true));
        }

        private void BeginEnemyTurn()
        {
            if (CheckDefeat()) return;

            State = BattleState.EnemyTurn;
            var enemyIndex = 0;
            ActiveUnit = GetNextAliveUnit(EnemyParty, ref enemyIndex);
            EventBus.Publish(new TurnStartedEvent(ActiveUnit, false));
        }

        public void ExecuteSkill(BattleUnit user, BattleUnit target, SkillData skill)
        {
            var effectiveness = ElementChart.GetEffectiveness(skill.element, target.WeaknessElement);
            int damage = CalculateDamage(user, target, skill, effectiveness);

            target.TakeDamage(damage);

            var result = new SkillResultEvent(user, target, skill, damage, effectiveness);
            EventBus.Publish(result);

            if (effectiveness == Effectiveness.Weak)
            {
                target.IsDown = true;
                EventBus.Publish(new WeaknessHitEvent(user, target));

                if (AllEnemiesDown())
                {
                    TriggerCatharsisBreak();
                    return;
                }

                State = BattleState.ResonanceTurn;
                EventBus.Publish(new ResonanceTurnEvent(user));
                return;
            }

            AdvanceTurn();
        }

        private void TriggerCatharsisBreak()
        {
            State = BattleState.CatharsisBreak;
            int totalDamage = 0;
            foreach (var player in PlayerParty)
            {
                if (!player.IsAlive) continue;
                foreach (var enemy in EnemyParty)
                {
                    if (!enemy.IsAlive) continue;
                    int dmg = player.Attack * 2;
                    enemy.TakeDamage(dmg);
                    totalDamage += dmg;
                }
            }
            EventBus.Publish(new CatharsisBreakEvent(totalDamage));
            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            foreach (var enemy in EnemyParty) enemy.IsDown = false;
            foreach (var player in PlayerParty) player.IsDown = false;

            if (State == BattleState.PlayerTurn || State == BattleState.ResonanceTurn || State == BattleState.CatharsisBreak)
            {
                turnIndex++;
                if (turnIndex >= PlayerParty.Count)
                {
                    turnIndex = 0;
                    BeginEnemyTurn();
                }
                else
                {
                    BeginPlayerTurn();
                }
            }
            else
            {
                turnIndex = 0;
                BeginPlayerTurn();
            }
        }

        private bool AllEnemiesDown()
        {
            foreach (var enemy in EnemyParty)
                if (enemy.IsAlive && !enemy.IsDown)
                    return false;
            return true;
        }

        private bool CheckVictory()
        {
            foreach (var enemy in EnemyParty)
                if (enemy.IsAlive) return false;
            State = BattleState.Victory;
            EventBus.Publish(new BattleEndedEvent(true));
            return true;
        }

        private bool CheckDefeat()
        {
            foreach (var player in PlayerParty)
                if (player.IsAlive) return false;
            State = BattleState.Defeat;
            EventBus.Publish(new BattleEndedEvent(false));
            return true;
        }

        private BattleUnit GetNextAliveUnit(List<BattleUnit> units, ref int index)
        {
            for (int i = 0; i < units.Count; i++)
            {
                int idx = (index + i) % units.Count;
                if (units[idx].IsAlive)
                {
                    index = idx;
                    return units[idx];
                }
            }
            return null;
        }

        private int CalculateDamage(BattleUnit user, BattleUnit target, SkillData skill, Effectiveness eff)
        {
            float baseDamage = skill.power * (user.Attack / (float)Mathf.Max(target.Defense, 1));
            float multiplier = eff switch
            {
                Effectiveness.Weak => 1.5f,
                Effectiveness.Resist => 0.5f,
                Effectiveness.Null => 0f,
                Effectiveness.Absorb => -0.5f,
                _ => 1.0f
            };
            float variance = Random.Range(0.9f, 1.1f);
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier * variance));
        }
    }

    public readonly struct BattleStartedEvent { }
    public readonly struct BattleEndedEvent
    {
        public readonly bool Victory;
        public BattleEndedEvent(bool victory) { Victory = victory; }
    }

    public readonly struct TurnStartedEvent
    {
        public readonly BattleUnit Unit;
        public readonly bool IsPlayer;
        public TurnStartedEvent(BattleUnit unit, bool isPlayer) { Unit = unit; IsPlayer = isPlayer; }
    }

    public readonly struct SkillResultEvent
    {
        public readonly BattleUnit User;
        public readonly BattleUnit Target;
        public readonly SkillData Skill;
        public readonly int Damage;
        public readonly Effectiveness Effectiveness;
        public SkillResultEvent(BattleUnit user, BattleUnit target, SkillData skill, int damage, Effectiveness eff)
        { User = user; Target = target; Skill = skill; Damage = damage; Effectiveness = eff; }
    }

    public readonly struct WeaknessHitEvent
    {
        public readonly BattleUnit Attacker;
        public readonly BattleUnit Target;
        public WeaknessHitEvent(BattleUnit attacker, BattleUnit target) { Attacker = attacker; Target = target; }
    }

    public readonly struct ResonanceTurnEvent
    {
        public readonly BattleUnit Unit;
        public ResonanceTurnEvent(BattleUnit unit) { Unit = unit; }
    }

    public readonly struct CatharsisBreakEvent
    {
        public readonly int TotalDamage;
        public CatharsisBreakEvent(int totalDamage) { TotalDamage = totalDamage; }
    }
}
