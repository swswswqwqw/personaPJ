using System;
using System.Collections.Generic;
using System.Linq;
using Amane.Core;

namespace Amane.Battle
{
    public enum BattlePhase
    {
        Start,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat,
        Fled
    }

    public sealed class BattleManager
    {
        private readonly EventChannel _events;
        private readonly TurnSystem _turns = new();

        public List<Combatant> Party { get; } = new();
        public List<Combatant> Enemies { get; } = new();
        public BattlePhase Phase { get; private set; }
        public Combatant ActiveCombatant => _turns.Current;
        public int KotsugiChain { get; private set; }

        public event Action<BattlePhase> OnPhaseChanged;
        public event Action<HitResult> OnHit;
        public event Action OnAllOutConfession;
        public event Action<Combatant, Combatant, float> OnKotsugi;

        public BattleManager(EventChannel events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void StartBattle(List<Combatant> party, List<Combatant> enemies)
        {
            Party.Clear();
            Party.AddRange(party);
            Enemies.Clear();
            Enemies.AddRange(enemies);
            KotsugiChain = 0;

            foreach (var c in AllCombatants())
                c.SetDown(false);

            SetPhase(BattlePhase.Start);
            BeginNewRound();
        }

        public void ExecuteAction(BattleAction action)
        {
            if (action == null) return;

            switch (action.Type)
            {
                case ActionType.Skill:
                    ExecuteSkill(action);
                    break;
                case ActionType.Guard:
                    break;
                case ActionType.KotsugiPass:
                    ExecuteKotsugi(action);
                    return;
                case ActionType.Flee:
                    SetPhase(BattlePhase.Fled);
                    return;
            }

            if (CheckBattleEnd()) return;

            if (Phase == BattlePhase.PlayerTurn || Phase == BattlePhase.EnemyTurn)
                AdvanceTurn();
        }

        public bool CanAllOutConfession()
        {
            return Enemies.Where(e => e.IsAlive).All(e => e.IsDown);
        }

        public void TriggerAllOutConfession()
        {
            if (!CanAllOutConfession()) return;

            OnAllOutConfession?.Invoke();

            foreach (var enemy in Enemies.Where(e => e.IsAlive))
            {
                int totalAtk = Party.Where(p => p.IsAlive).Sum(p => p.Attack);
                int dmg = Math.Max(1, (int)(totalAtk * 0.8f));
                enemy.TakeDamage(dmg);
            }

            foreach (var enemy in Enemies)
                enemy.SetDown(false);

            CheckBattleEnd();
        }

        public float GetKotsugiBonus()
        {
            if (KotsugiChain <= 0) return 0f;
            return 0.5f + 0.25f * (KotsugiChain - 1);
        }

        private void ExecuteSkill(BattleAction action)
        {
            var skill = action.Skill ?? Skill.MeleeAttack;
            if (!action.Actor.SpendSp(skill.SpCost))
                skill = Skill.MeleeAttack;

            float bonus = GetKotsugiBonus();
            bool anyOneMore = false;

            foreach (var target in action.Targets)
            {
                if (!target.IsAlive) continue;
                var result = DamageCalculator.Calculate(action.Actor, target, skill, bonus);
                OnHit?.Invoke(result);

                if (result.TriggersOneMore)
                    anyOneMore = true;
            }

            if (anyOneMore && action.Actor.IsPlayer && CanAllOutConfession())
            {
                // All enemies down - player can choose All-Out Confession
                return;
            }

            if (anyOneMore)
            {
                _turns.InsertOneMore(action.Actor);
            }
            else
            {
                KotsugiChain = 0;
            }
        }

        private void ExecuteKotsugi(BattleAction action)
        {
            var receiver = action.Targets?.FirstOrDefault();
            if (receiver == null || !receiver.IsAlive) return;

            KotsugiChain++;
            float bonus = GetKotsugiBonus();
            OnKotsugi?.Invoke(action.Actor, receiver, bonus);
            _turns.InsertOneMore(receiver);
            AdvanceTurn();
        }

        private void BeginNewRound()
        {
            _turns.BuildOrder(AllCombatants());
            foreach (var c in AllCombatants().Where(c => c.IsAlive))
                c.SetDown(false);

            SetCurrentPhase();
        }

        private void AdvanceTurn()
        {
            var next = _turns.Advance();
            if (next == null || _turns.IsRoundOver)
            {
                BeginNewRound();
                return;
            }
            SetCurrentPhase();
        }

        private void SetCurrentPhase()
        {
            var current = _turns.Current;
            if (current == null) return;
            SetPhase(current.IsPlayer ? BattlePhase.PlayerTurn : BattlePhase.EnemyTurn);
        }

        private bool CheckBattleEnd()
        {
            if (Party.All(p => !p.IsAlive))
            {
                SetPhase(BattlePhase.Defeat);
                return true;
            }
            if (Enemies.All(e => !e.IsAlive))
            {
                SetPhase(BattlePhase.Victory);
                return true;
            }
            return false;
        }

        private void SetPhase(BattlePhase phase)
        {
            Phase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        private IEnumerable<Combatant> AllCombatants() => Party.Concat(Enemies);
    }
}
