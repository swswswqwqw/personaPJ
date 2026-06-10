using System;
using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Core;
using ArcadiaOfEchoes.Data;

namespace ArcadiaOfEchoes.Battle
{
    public enum BattleState
    {
        Init,
        TurnStart,
        CommandSelect,
        ActionExecute,
        TurnEnd,
        Victory,
        Defeat,
        Escape
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private readonly StateMachine<BattleState> _stateMachine = new();
        private readonly List<BattleUnit> _partyUnits = new();
        private readonly List<BattleUnit> _enemyUnits = new();
        private readonly List<BattleUnit> _turnOrder = new();

        public IReadOnlyList<BattleUnit> PartyUnits => _partyUnits;
        public IReadOnlyList<BattleUnit> EnemyUnits => _enemyUnits;
        public BattleUnit ActiveUnit { get; private set; }
        public bool IsOneMoreActive { get; private set; }

        public event Action<BattleState> OnBattleStateChanged;
        public event Action<BattleUnit, BattleUnit, int, bool> OnDamageDealt;
        public event Action<BattleUnit> OnUnitDowned;
        public event Action OnAllEnemiesDowned;
        public event Action OnHarmonyBreak;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeBattle(List<BattleUnit> party, List<BattleUnit> enemies)
        {
            _partyUnits.Clear();
            _partyUnits.AddRange(party);
            _enemyUnits.Clear();
            _enemyUnits.AddRange(enemies);

            CalculateTurnOrder();

            EventBus.Publish(new BattleStartEvent { EncounterId = "" });
        }

        public void CalculateTurnOrder()
        {
            _turnOrder.Clear();
            _turnOrder.AddRange(_partyUnits);
            _turnOrder.AddRange(_enemyUnits);
            _turnOrder.RemoveAll(u => u.IsDead);
            _turnOrder.Sort((a, b) => b.Stats.Agility.CompareTo(a.Stats.Agility));
        }

        public DamageResult CalculateDamage(BattleUnit attacker, BattleUnit defender, SkillData skill)
        {
            var result = new DamageResult();

            var affinity = defender.GetAffinity(skill.Element);
            result.Affinity = affinity;

            switch (affinity)
            {
                case AffinityType.Absorb:
                    result.Damage = -(int)(skill.BasePower * 0.5f);
                    result.IsAbsorbed = true;
                    return result;
                case AffinityType.Null:
                    result.Damage = 0;
                    result.IsNulled = true;
                    return result;
                case AffinityType.Reflect:
                    result.IsReflected = true;
                    result.Damage = CalculateRawDamage(attacker, attacker, skill);
                    return result;
            }

            int rawDamage = CalculateRawDamage(attacker, defender, skill);

            float affinityMult = affinity switch
            {
                AffinityType.Weak => 1.5f,
                AffinityType.Resist => 0.5f,
                _ => 1.0f
            };

            bool isCritical = UnityEngine.Random.value < skill.CriticalRate;
            float critMult = isCritical ? 1.5f : 1.0f;

            float resonanceMult = 1.0f;
            if (attacker.IsPlayerUnit)
            {
                resonanceMult = Social.ResonanceManager.Instance?.GetResonanceDamageMultiplier(attacker.CharacterId) ?? 1.0f;
            }

            result.Damage = Mathf.Max(1, (int)(rawDamage * affinityMult * critMult * resonanceMult));
            result.IsCritical = isCritical;
            result.IsWeakness = affinity == AffinityType.Weak;
            result.GrantsOnMore = result.IsWeakness || result.IsCritical;

            return result;
        }

        public void ApplyDamage(BattleUnit target, DamageResult result)
        {
            target.CurrentHP = Mathf.Clamp(target.CurrentHP - result.Damage, 0, target.Stats.HP);

            if (result.IsWeakness || result.IsCritical)
            {
                target.IsDown = true;
                OnUnitDowned?.Invoke(target);
            }

            if (target.IsDead)
            {
                target.IsDown = true;
            }

            OnDamageDealt?.Invoke(ActiveUnit, target, result.Damage, result.IsWeakness);

            if (result.GrantsOnMore)
            {
                IsOneMoreActive = true;
            }

            CheckAllEnemiesDowned();
        }

        public bool CanHarmonyBreak()
        {
            return _enemyUnits.TrueForAll(e => e.IsDown || e.IsDead);
        }

        public void ExecuteHarmonyBreak()
        {
            foreach (var enemy in _enemyUnits)
            {
                if (enemy.IsDead) continue;
                int damage = (int)(enemy.Stats.HP * 0.25f);
                foreach (var ally in _partyUnits)
                {
                    if (ally.IsDead) continue;
                    float resonanceMult = Social.ResonanceManager.Instance?.GetResonanceDamageMultiplier(ally.CharacterId) ?? 1.0f;
                    damage += (int)(ally.Stats.Strength * 2 * resonanceMult);
                }
                enemy.CurrentHP = Mathf.Max(0, enemy.CurrentHP - damage);
            }

            OnHarmonyBreak?.Invoke();
        }

        public bool CheckBattleEnd()
        {
            if (_enemyUnits.TrueForAll(e => e.IsDead))
            {
                EventBus.Publish(new BattleEndEvent { Victory = true });
                return true;
            }
            if (_partyUnits.TrueForAll(p => p.IsDead))
            {
                EventBus.Publish(new BattleEndEvent { Victory = false });
                return true;
            }
            return false;
        }

        private int CalculateRawDamage(BattleUnit attacker, BattleUnit defender, SkillData skill)
        {
            float attackStat = skill.Type == SkillType.Attack && skill.Element == ElementType.Strike
                ? attacker.Stats.Strength
                : attacker.Stats.Magic;
            float defenseStat = defender.Stats.Endurance;

            return Mathf.Max(1, (int)(skill.BasePower * (attackStat / (defenseStat * 0.5f + 1f))));
        }

        private void CheckAllEnemiesDowned()
        {
            if (CanHarmonyBreak())
            {
                OnAllEnemiesDowned?.Invoke();
            }
        }
    }

    public class BattleUnit
    {
        public string CharacterId;
        public string UnitName;
        public bool IsPlayerUnit;
        public CharacterStats Stats;
        public int CurrentHP;
        public int CurrentSP;
        public bool IsDown;
        public bool IsDead => CurrentHP <= 0;
        public List<SkillData> Skills = new();
        public List<ElementAffinity> Affinities = new();

        public AffinityType GetAffinity(ElementType element)
        {
            var match = Affinities.Find(a => a.Element == element);
            return match?.Affinity ?? AffinityType.Normal;
        }
    }

    public struct DamageResult
    {
        public int Damage;
        public bool IsWeakness;
        public bool IsCritical;
        public bool IsNulled;
        public bool IsAbsorbed;
        public bool IsReflected;
        public bool GrantsOnMore;
        public AffinityType Affinity;
    }
}
