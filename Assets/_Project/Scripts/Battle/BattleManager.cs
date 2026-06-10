using UnityEngine;
using System.Collections.Generic;
using Astra.Core;
using Astra.Data;

namespace Astra.Battle
{
    public enum BattleState
    {
        Init,
        PlayerTurn,
        EnemyTurn,
        ResonanceBurst,
        Victory,
        Defeat,
        Escape
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private StateMachine<BattleState> _stateMachine;
        private List<BattleUnit> _playerParty = new();
        private List<BattleUnit> _enemyParty = new();

        private int _echoStock = 0;
        private int _resonanceGauge = 0;

        public const int MaxEchoStock = 3;
        public const int ResonanceGaugeMax = 100;

        public BattleState CurrentState => _stateMachine.CurrentState;
        public int EchoStock => _echoStock;
        public int ResonanceGauge => _resonanceGauge;
        public IReadOnlyList<BattleUnit> PlayerParty => _playerParty;
        public IReadOnlyList<BattleUnit> EnemyParty => _enemyParty;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _stateMachine = new StateMachine<BattleState>(BattleState.Init);
        }

        public void StartBattle(List<CharacterData> party, List<EnemyData> enemies)
        {
            _playerParty.Clear();
            _enemyParty.Clear();
            _echoStock = 0;
            _resonanceGauge = 0;

            foreach (var data in party)
                _playerParty.Add(new BattleUnit(data));
            foreach (var data in enemies)
                _enemyParty.Add(new BattleUnit(data));

            _stateMachine.TransitionTo(BattleState.PlayerTurn);
            EventBus.Publish(new BattleStartedEvent());
        }

        public AttackResult ExecuteSkill(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            var result = DamageCalculator.Calculate(attacker, target, skill);

            target.TakeDamage(result.Damage);
            attacker.ConsumeSP(skill.spCost);

            if (result.HitWeakness)
            {
                AddEcho(1);
                AddResonance(15);
                EventBus.Publish(new WeaknessHitEvent(attacker, target, skill));
            }

            if (result.IsCritical)
            {
                AddEcho(1);
                EventBus.Publish(new CriticalHitEvent(attacker, target, skill));
            }

            if (result.WasResisted)
            {
                ConsumeEcho(1);
            }

            EventBus.Publish(new SkillExecutedEvent(attacker, target, skill, result));

            CheckBattleEnd();
            return result;
        }

        public bool CanResonanceBurst() => _resonanceGauge >= ResonanceGaugeMax;

        public void ExecuteResonanceBurst()
        {
            if (!CanResonanceBurst()) return;

            _resonanceGauge = 0;
            _stateMachine.TransitionTo(BattleState.ResonanceBurst);
            EventBus.Publish(new ResonanceBurstEvent(_playerParty));
        }

        private void AddEcho(int amount)
        {
            _echoStock = Mathf.Min(_echoStock + amount, MaxEchoStock);
            EventBus.Publish(new EchoChangedEvent(_echoStock));
        }

        private void ConsumeEcho(int amount)
        {
            _echoStock = Mathf.Max(_echoStock - amount, 0);
            EventBus.Publish(new EchoChangedEvent(_echoStock));
        }

        private void AddResonance(int amount)
        {
            _resonanceGauge = Mathf.Min(_resonanceGauge + amount, ResonanceGaugeMax);
            EventBus.Publish(new ResonanceGaugeChangedEvent(_resonanceGauge));
        }

        private void CheckBattleEnd()
        {
            if (_enemyParty.TrueForAll(e => e.IsDead))
            {
                _stateMachine.TransitionTo(BattleState.Victory);
                EventBus.Publish(new BattleEndedEvent(true));
            }
            else if (_playerParty.TrueForAll(p => p.IsDead))
            {
                _stateMachine.TransitionTo(BattleState.Defeat);
                EventBus.Publish(new BattleEndedEvent(false));
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public readonly struct BattleStartedEvent { }
    public readonly struct BattleEndedEvent
    {
        public readonly bool PlayerWon;
        public BattleEndedEvent(bool playerWon) { PlayerWon = playerWon; }
    }
    public readonly struct WeaknessHitEvent
    {
        public readonly BattleUnit Attacker;
        public readonly BattleUnit Target;
        public readonly SkillData Skill;
        public WeaknessHitEvent(BattleUnit a, BattleUnit t, SkillData s) { Attacker = a; Target = t; Skill = s; }
    }
    public readonly struct CriticalHitEvent
    {
        public readonly BattleUnit Attacker;
        public readonly BattleUnit Target;
        public readonly SkillData Skill;
        public CriticalHitEvent(BattleUnit a, BattleUnit t, SkillData s) { Attacker = a; Target = t; Skill = s; }
    }
    public readonly struct SkillExecutedEvent
    {
        public readonly BattleUnit Attacker;
        public readonly BattleUnit Target;
        public readonly SkillData Skill;
        public readonly AttackResult Result;
        public SkillExecutedEvent(BattleUnit a, BattleUnit t, SkillData s, AttackResult r)
        { Attacker = a; Target = t; Skill = s; Result = r; }
    }
    public readonly struct EchoChangedEvent
    {
        public readonly int NewStock;
        public EchoChangedEvent(int stock) { NewStock = stock; }
    }
    public readonly struct ResonanceGaugeChangedEvent
    {
        public readonly int NewGauge;
        public ResonanceGaugeChangedEvent(int gauge) { NewGauge = gauge; }
    }
    public readonly struct ResonanceBurstEvent
    {
        public readonly List<BattleUnit> Party;
        public ResonanceBurstEvent(List<BattleUnit> party) { Party = party; }
    }
}
