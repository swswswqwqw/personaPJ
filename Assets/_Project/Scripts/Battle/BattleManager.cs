using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Battle
{
    public enum BattleState
    {
        Init,
        PlayerTurn,
        EnemyTurn,
        Encore,
        GrandFinale,
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
        private int _currentActorIndex;
        private BattleUnit _encoreUnit;

        public IReadOnlyList<BattleUnit> PlayerParty => _playerParty;
        public IReadOnlyList<BattleUnit> EnemyParty => _enemyParty;
        public BattleState CurrentState => _stateMachine.CurrentStateId;

        public event Action OnBattleStart;
        public event Action<bool> OnBattleEnd;
        public event Action<BattleUnit> OnTurnStart;
        public event Action<DamageResult> OnDamageDealt;
        public event Action OnEncoreTriggered;
        public event Action OnGrandFinaleReady;

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
            _playerParty = players;
            _enemyParty = enemies;
            _currentActorIndex = 0;

            _stateMachine = new StateMachine<BattleState>();
            _stateMachine.AddState(BattleState.Init, new BattleInitState(this));
            _stateMachine.AddState(BattleState.PlayerTurn, new PlayerTurnState(this));
            _stateMachine.AddState(BattleState.EnemyTurn, new EnemyTurnState(this));
            _stateMachine.AddState(BattleState.Encore, new EncoreState(this));
            _stateMachine.AddState(BattleState.GrandFinale, new GrandFinaleState(this));
            _stateMachine.AddState(BattleState.Victory, new BattleEndState(this, true));
            _stateMachine.AddState(BattleState.Defeat, new BattleEndState(this, false));

            _stateMachine.SetState(BattleState.Init);
            OnBattleStart?.Invoke();

            GameManager.Instance?.ChangePhase(GamePhase.Battle);
        }

        private void Update()
        {
            _stateMachine?.Update(UnityEngine.Time.deltaTime);
        }

        public DamageResult CalculateDamage(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            float baseDamage = skill.element == Element.Physical
                ? skill.basePower * (attacker.Strength / (float)target.Endurance)
                : skill.basePower * (attacker.Magic / (float)target.Endurance);

            var affinity = GetAffinity(target, skill.element);
            bool isWeak = affinity == ElementAffinity.Weak;
            bool isCritical = UnityEngine.Random.Range(0, 100) < skill.criticalRate;

            float multiplier = affinity switch
            {
                ElementAffinity.Weak => 1.5f,
                ElementAffinity.Resist => 0.5f,
                ElementAffinity.Null => 0f,
                ElementAffinity.Absorb => -0.5f,
                ElementAffinity.Repel => -1f,
                _ => 1f
            };

            if (isCritical) multiplier *= 1.5f;

            float variance = UnityEngine.Random.Range(0.9f, 1.1f);
            int finalDamage = Mathf.RoundToInt(baseDamage * multiplier * variance);

            bool triggerEncore = isWeak || isCritical;
            bool triggerDown = isWeak;

            return new DamageResult
            {
                Damage = finalDamage,
                IsWeak = isWeak,
                IsCritical = isCritical,
                IsAbsorbed = affinity == ElementAffinity.Absorb,
                IsRepelled = affinity == ElementAffinity.Repel,
                IsNulled = affinity == ElementAffinity.Null,
                TriggerEncore = triggerEncore,
                TargetDowned = triggerDown
            };
        }

        private ElementAffinity GetAffinity(BattleUnit target, Element element)
        {
            if (target.CharacterData == null) return ElementAffinity.Normal;

            return element switch
            {
                Element.Fire => target.CharacterData.fireAffinity,
                Element.Ice => target.CharacterData.iceAffinity,
                Element.Wind => target.CharacterData.windAffinity,
                Element.Lightning => target.CharacterData.lightningAffinity,
                Element.Light => target.CharacterData.lightAffinity,
                Element.Dark => target.CharacterData.darkAffinity,
                Element.Resonance => target.CharacterData.resonanceAffinity,
                _ => ElementAffinity.Normal
            };
        }

        public void ApplyDamage(BattleUnit target, DamageResult result)
        {
            if (result.IsAbsorbed)
                target.Heal(Mathf.Abs(result.Damage));
            else if (!result.IsNulled && !result.IsRepelled)
                target.TakeDamage(result.Damage);

            if (result.TargetDowned)
                target.IsDown = true;

            OnDamageDealt?.Invoke(result);

            if (result.TriggerEncore)
                OnEncoreTriggered?.Invoke();

            CheckGrandFinale();
            CheckBattleEnd();
        }

        private void CheckGrandFinale()
        {
            bool allDown = true;
            foreach (var enemy in _enemyParty)
            {
                if (enemy.IsAlive && !enemy.IsDown)
                {
                    allDown = false;
                    break;
                }
            }
            if (allDown)
                OnGrandFinaleReady?.Invoke();
        }

        private void CheckBattleEnd()
        {
            bool allEnemiesDead = true;
            foreach (var enemy in _enemyParty)
            {
                if (enemy.IsAlive) { allEnemiesDead = false; break; }
            }
            if (allEnemiesDead)
            {
                _stateMachine.SetState(BattleState.Victory);
                return;
            }

            bool allPlayersDead = true;
            foreach (var player in _playerParty)
            {
                if (player.IsAlive) { allPlayersDead = false; break; }
            }
            if (allPlayersDead)
                _stateMachine.SetState(BattleState.Defeat);
        }

        public void TransitionTo(BattleState state) => _stateMachine.SetState(state);
    }

    public struct DamageResult
    {
        public int Damage;
        public bool IsWeak;
        public bool IsCritical;
        public bool IsAbsorbed;
        public bool IsRepelled;
        public bool IsNulled;
        public bool TriggerEncore;
        public bool TargetDowned;
    }

    #region Battle States

    public class BattleInitState : IState
    {
        private readonly BattleManager _manager;
        public BattleInitState(BattleManager manager) => _manager = manager;

        public void Enter() => _manager.TransitionTo(BattleState.PlayerTurn);
        public void Update(float deltaTime) { }
        public void Exit() { }
    }

    public class PlayerTurnState : IState
    {
        private readonly BattleManager _manager;
        public PlayerTurnState(BattleManager manager) => _manager = manager;

        public void Enter()
        {
            // UI waits for player input
        }

        public void Update(float deltaTime) { }
        public void Exit() { }
    }

    public class EnemyTurnState : IState
    {
        private readonly BattleManager _manager;
        public EnemyTurnState(BattleManager manager) => _manager = manager;

        public void Enter()
        {
            // Enemy AI selects action
        }

        public void Update(float deltaTime) { }
        public void Exit() { }
    }

    public class EncoreState : IState
    {
        private readonly BattleManager _manager;
        public EncoreState(BattleManager manager) => _manager = manager;

        public void Enter()
        {
            // ENCORE! animation then back to player turn
        }

        public void Update(float deltaTime) { }
        public void Exit() { }
    }

    public class GrandFinaleState : IState
    {
        private readonly BattleManager _manager;
        public GrandFinaleState(BattleManager manager) => _manager = manager;

        public void Enter()
        {
            // Grand Finale cut-in animation
        }

        public void Update(float deltaTime) { }
        public void Exit() { }
    }

    public class BattleEndState : IState
    {
        private readonly BattleManager _manager;
        private readonly bool _isVictory;

        public BattleEndState(BattleManager manager, bool isVictory)
        {
            _manager = manager;
            _isVictory = isVictory;
        }

        public void Enter()
        {
            // Victory/Defeat screen
        }

        public void Update(float deltaTime) { }
        public void Exit() { }
    }

    #endregion
}
