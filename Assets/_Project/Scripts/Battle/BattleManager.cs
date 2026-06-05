using System;
using System.Collections.Generic;
using System.Linq;
using AstralEchoes.Core;
using AstralEchoes.Data;

namespace AstralEchoes.Battle
{
    public sealed class BattleManager
    {
        static BattleManager _instance;
        public static BattleManager Instance => _instance ??= new BattleManager();

        public BattlePhase CurrentPhase { get; private set; } = BattlePhase.Inactive;
        public List<BattleUnit> PartyUnits { get; } = new();
        public List<BattleUnit> EnemyUnits { get; } = new();
        public BattleUnit ActiveUnit { get; private set; }
        public bool OneMoreActive { get; private set; }

        List<BattleUnit> _turnOrder = new();
        int _turnIndex;

        public event Action<BattlePhase> OnPhaseChanged;
        public event Action<BattleUnit> OnTurnStarted;
        public event Action<DamageResult> OnDamageDealt;
        public event Action OnFullResonanceReady;
        public event Action<bool> OnBattleEnded;

        BattleManager() { }

        public void StartBattle(List<BattleUnit> party, List<BattleUnit> enemies)
        {
            PartyUnits.Clear();
            EnemyUnits.Clear();
            PartyUnits.AddRange(party);
            EnemyUnits.AddRange(enemies);

            GameManager.Instance.ChangeState(GameState.Battle);
            ChangePhase(BattlePhase.Start);
            CalculateTurnOrder();
            ChangePhase(BattlePhase.PlayerTurn);
            BeginNextTurn();

            EventBus.Publish(new BattleStartedEvent { EncounterId = "test" });
        }

        void CalculateTurnOrder()
        {
            _turnOrder = PartyUnits.Concat(EnemyUnits)
                .Where(u => u.IsAlive)
                .OrderByDescending(u => u.Stats.Agility)
                .ToList();
            _turnIndex = 0;
        }

        void BeginNextTurn()
        {
            if (CheckBattleEnd()) return;

            if (_turnIndex >= _turnOrder.Count)
            {
                CalculateTurnOrder();
            }

            ActiveUnit = _turnOrder[_turnIndex];
            _turnIndex++;

            if (!ActiveUnit.IsAlive)
            {
                BeginNextTurn();
                return;
            }

            ActiveUnit.ResetTurnState();

            if (ActiveUnit.IsEnemy)
                ChangePhase(BattlePhase.EnemyTurn);
            else
                ChangePhase(BattlePhase.PlayerTurn);

            OnTurnStarted?.Invoke(ActiveUnit);
        }

        public DamageResult ExecuteAttack(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            var result = DamageCalculator.Calculate(attacker, target, skill);

            target.ApplyDamage(result.FinalDamage);
            OnDamageDealt?.Invoke(result);

            if (result.IsWeakness || result.IsCritical)
            {
                target.SetDown(true);

                EventBus.Publish(new WeaknessHitEvent
                {
                    AttackerId = attacker.Id,
                    TargetId = target.Id,
                    AttributeUsed = skill.Attribute
                });

                if (AllEnemiesDown())
                {
                    OnFullResonanceReady?.Invoke();
                    EventBus.Publish(new FullResonanceReadyEvent
                    {
                        DownedEnemyCount = EnemyUnits.Count(e => e.IsDown && e.IsAlive)
                    });
                }
                else
                {
                    OneMoreActive = true;
                    OnTurnStarted?.Invoke(attacker);
                    return result;
                }
            }

            OneMoreActive = false;
            BeginNextTurn();
            return result;
        }

        public DamageResult ExecuteFullResonance()
        {
            int totalDamage = 0;
            foreach (var enemy in EnemyUnits.Where(e => e.IsAlive))
            {
                int damage = CalculateFullResonanceDamage(enemy);
                enemy.ApplyDamage(damage);
                totalDamage += damage;
            }

            foreach (var enemy in EnemyUnits)
                enemy.SetDown(false);

            var result = new DamageResult
            {
                FinalDamage = totalDamage,
                IsFullResonance = true
            };

            OnDamageDealt?.Invoke(result);
            OneMoreActive = false;
            BeginNextTurn();
            return result;
        }

        int CalculateFullResonanceDamage(BattleUnit target)
        {
            int partyPower = PartyUnits.Where(u => u.IsAlive).Sum(u => u.Stats.Strength + u.Stats.Magic);
            return (int)(partyPower * 1.5f) - target.Stats.Endurance / 2;
        }

        bool AllEnemiesDown() => EnemyUnits.Where(e => e.IsAlive).All(e => e.IsDown);

        bool CheckBattleEnd()
        {
            if (EnemyUnits.All(e => !e.IsAlive))
            {
                EndBattle(true);
                return true;
            }
            if (PartyUnits.All(p => !p.IsAlive))
            {
                EndBattle(false);
                return true;
            }
            return false;
        }

        void EndBattle(bool victory)
        {
            ChangePhase(victory ? BattlePhase.Victory : BattlePhase.Defeat);
            OnBattleEnded?.Invoke(victory);

            EventBus.Publish(new BattleEndedEvent
            {
                Victory = victory,
                ExpGained = victory ? CalculateExpReward() : 0,
                MoneyGained = victory ? CalculateMoneyReward() : 0
            });
        }

        int CalculateExpReward() => EnemyUnits.Sum(e => e.BaseExpReward);
        int CalculateMoneyReward() => EnemyUnits.Sum(e => e.BaseMoneyReward);

        void ChangePhase(BattlePhase phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }
    }

    public enum BattlePhase
    {
        Inactive,
        Start,
        PlayerTurn,
        EnemyTurn,
        FullResonance,
        Victory,
        Defeat
    }
}
