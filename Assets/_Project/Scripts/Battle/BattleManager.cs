using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Core;
using ArcadiaOfEchoes.Data;

namespace ArcadiaOfEchoes.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private StateMachine _battleStateMachine;
        private List<BattlerInstance> _partyMembers;
        private List<BattlerInstance> _enemies;
        private int _currentActorIndex;
        private int _echoRelayChain;

        public IReadOnlyList<BattlerInstance> PartyMembers => _partyMembers;
        public IReadOnlyList<BattlerInstance> Enemies => _enemies;
        public BattlerInstance CurrentActor => _turnOrder?[_currentActorIndex];
        public int EchoRelayChain => _echoRelayChain;
        public bool AllEnemiesDown => _enemies.TrueForAll(e => e.State == BattlerState.Down || e.State == BattlerState.Dead);

        private List<BattlerInstance> _turnOrder;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartBattle(List<CharacterData> party, List<CharacterData> enemies)
        {
            _partyMembers = new List<BattlerInstance>();
            _enemies = new List<BattlerInstance>();
            _echoRelayChain = 0;

            foreach (var c in party)
                _partyMembers.Add(new BattlerInstance(c, true));
            foreach (var e in enemies)
                _enemies.Add(new BattlerInstance(e, false));

            CalculateTurnOrder();
            _currentActorIndex = 0;

            EventBus.Publish(new BattleStartedEvent(_partyMembers, _enemies));
        }

        public TurnResult ExecuteAttack(BattlerInstance attacker, BattlerInstance target, SkillData skill)
        {
            var affinity = target.GetAffinity(skill.Element);

            int damage = CalculateDamage(attacker, target, skill, affinity);
            target.TakeDamage(damage);

            var result = TurnResult.Normal;

            switch (affinity)
            {
                case AffinityType.Weak:
                    target.State = BattlerState.Down;
                    result = TurnResult.OneMoreEcho;
                    EventBus.Publish(new WeaknessHitEvent(attacker, target, skill.Element));
                    break;
                case AffinityType.Null:
                    result = TurnResult.Blocked;
                    break;
                case AffinityType.Reflect:
                    attacker.TakeDamage(damage);
                    result = TurnResult.Reflected;
                    break;
                case AffinityType.Absorb:
                    target.Heal(damage * 2);
                    result = TurnResult.Blocked;
                    break;
            }

            EventBus.Publish(new AttackExecutedEvent(attacker, target, skill, damage, result));

            if (AllEnemiesDown)
                EventBus.Publish(new AllEnemiesDownEvent());

            return result;
        }

        public void ExecuteEchoRelay(BattlerInstance from, BattlerInstance to)
        {
            _echoRelayChain++;
            float multiplier = _echoRelayChain switch
            {
                1 => 1.25f,
                2 => 1.5f,
                _ => 2.0f
            };

            to.SetRelayMultiplier(multiplier);
            EventBus.Publish(new EchoRelayEvent(from, to, _echoRelayChain, multiplier));

            if (_echoRelayChain >= 4)
                EventBus.Publish(new PerfectEchoEvent());
        }

        public void ExecuteUnisonBreak()
        {
            EventBus.Publish(new UnisonBreakEvent(_partyMembers));
        }

        public void EndTurn()
        {
            _echoRelayChain = 0;
            _currentActorIndex++;

            if (_currentActorIndex >= _turnOrder.Count)
            {
                _currentActorIndex = 0;
                CalculateTurnOrder();
                EventBus.Publish(new NewRoundEvent());
            }

            foreach (var battler in _turnOrder)
                battler.ResetRelayMultiplier();

            EventBus.Publish(new TurnChangedEvent(CurrentActor));
        }

        private void CalculateTurnOrder()
        {
            _turnOrder = new List<BattlerInstance>();
            _turnOrder.AddRange(_partyMembers.FindAll(b => b.State != BattlerState.Dead));
            _turnOrder.AddRange(_enemies.FindAll(b => b.State != BattlerState.Dead));
            _turnOrder.Sort((a, b) => b.Speed.CompareTo(a.Speed));
        }

        private int CalculateDamage(BattlerInstance attacker, BattlerInstance target, SkillData skill, AffinityType affinity)
        {
            float baseDamage = (attacker.Attack + skill.Power) - target.Defense * 0.5f;
            baseDamage *= attacker.RelayMultiplier;

            baseDamage *= affinity switch
            {
                AffinityType.Weak => 1.5f,
                AffinityType.Resist => 0.5f,
                _ => 1.0f
            };

            baseDamage *= UnityEngine.Random.Range(0.9f, 1.1f);
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage));
        }
    }

    public class BattlerInstance
    {
        public CharacterData Data { get; }
        public bool IsParty { get; }
        public int CurrentHP { get; private set; }
        public int CurrentSP { get; private set; }
        public BattlerState State { get; set; }
        public float RelayMultiplier { get; private set; } = 1f;
        public int Attack => Data.BaseAttack;
        public int Defense => Data.BaseDefense;
        public int Speed => Data.BaseSpeed;

        public BattlerInstance(CharacterData data, bool isParty)
        {
            Data = data;
            IsParty = isParty;
            CurrentHP = data.MaxHP;
            CurrentSP = data.MaxSP;
            State = BattlerState.Normal;
        }

        public AffinityType GetAffinity(ElementType element)
        {
            if (Data.Affinities == null) return AffinityType.Normal;
            foreach (var a in Data.Affinities)
            {
                if (a.Element == element) return a.Affinity;
            }
            return AffinityType.Normal;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
            if (CurrentHP <= 0) State = BattlerState.Dead;
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(Data.MaxHP, CurrentHP + amount);
        }

        public void SetRelayMultiplier(float multiplier) => RelayMultiplier = multiplier;
        public void ResetRelayMultiplier() => RelayMultiplier = 1f;
    }

    // Battle Events
    public readonly struct BattleStartedEvent
    {
        public readonly List<BattlerInstance> Party;
        public readonly List<BattlerInstance> Enemies;
        public BattleStartedEvent(List<BattlerInstance> party, List<BattlerInstance> enemies) { Party = party; Enemies = enemies; }
    }

    public readonly struct AttackExecutedEvent
    {
        public readonly BattlerInstance Attacker;
        public readonly BattlerInstance Target;
        public readonly SkillData Skill;
        public readonly int Damage;
        public readonly TurnResult Result;
        public AttackExecutedEvent(BattlerInstance a, BattlerInstance t, SkillData s, int d, TurnResult r) { Attacker = a; Target = t; Skill = s; Damage = d; Result = r; }
    }

    public readonly struct WeaknessHitEvent
    {
        public readonly BattlerInstance Attacker;
        public readonly BattlerInstance Target;
        public readonly ElementType Element;
        public WeaknessHitEvent(BattlerInstance a, BattlerInstance t, ElementType e) { Attacker = a; Target = t; Element = e; }
    }

    public readonly struct EchoRelayEvent
    {
        public readonly BattlerInstance From;
        public readonly BattlerInstance To;
        public readonly int ChainCount;
        public readonly float Multiplier;
        public EchoRelayEvent(BattlerInstance f, BattlerInstance t, int c, float m) { From = f; To = t; ChainCount = c; Multiplier = m; }
    }

    public readonly struct PerfectEchoEvent { }
    public readonly struct AllEnemiesDownEvent { }
    public readonly struct UnisonBreakEvent
    {
        public readonly List<BattlerInstance> Party;
        public UnisonBreakEvent(List<BattlerInstance> party) { Party = party; }
    }
    public readonly struct TurnChangedEvent
    {
        public readonly BattlerInstance CurrentActor;
        public TurnChangedEvent(BattlerInstance actor) { CurrentActor = actor; }
    }
    public readonly struct NewRoundEvent { }
}
