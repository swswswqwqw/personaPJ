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
        ResonanceChance,
        FullResonanceAttack,
        Victory,
        Defeat,
        Escape
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private readonly StateMachine stateMachine = new();

        public BattleState CurrentState { get; private set; }
        public List<BattleUnit> PartyUnits { get; private set; } = new();
        public List<BattleUnit> EnemyUnits { get; private set; } = new();
        public List<BattleUnit> TurnOrder { get; private set; } = new();
        public int CurrentTurnIndex { get; private set; }
        public bool IsOneMore { get; private set; }

        public event Action<BattleState> OnBattleStateChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartBattle(List<CharacterData> party, List<EnemyData> enemies)
        {
            PartyUnits.Clear();
            EnemyUnits.Clear();

            foreach (var member in party)
                PartyUnits.Add(new BattleUnit(member));

            foreach (var enemy in enemies)
                EnemyUnits.Add(new BattleUnit(enemy));

            CalculateTurnOrder();
            ChangeState(BattleState.Init);
            GameEventBus.Publish(new BattleStartedEvent());
        }

        public AttackResult ExecuteAbility(BattleUnit attacker, BattleUnit target, AbilityData ability)
        {
            var result = DamageCalculator.Calculate(attacker, target, ability);

            target.CurrentHP -= result.Damage;
            attacker.CurrentSP -= ability.spCost;

            if (result.HitWeakness)
            {
                target.IsDown = true;
                IsOneMore = true;
                GameEventBus.Publish(new ResonanceEvent(attacker, target, ability.element));
            }

            GameEventBus.Publish(new AbilityExecutedEvent(attacker, target, ability, result));

            if (CheckAllEnemiesDown())
            {
                ChangeState(BattleState.FullResonanceAttack);
            }
            else if (CheckBattleEnd())
            {
                ChangeState(AllEnemiesDefeated() ? BattleState.Victory : BattleState.Defeat);
            }
            else if (IsOneMore)
            {
                ChangeState(BattleState.ResonanceChance);
            }

            return result;
        }

        public void ExecuteFullResonanceAttack()
        {
            int totalDamage = 0;
            int bondBonus = CalculateBondBonus();

            foreach (var enemy in EnemyUnits)
            {
                if (!enemy.IsAlive) continue;
                int damage = CalculateFullResonanceDamage(enemy, bondBonus);
                enemy.CurrentHP -= damage;
                totalDamage += damage;
            }

            foreach (var enemy in EnemyUnits)
                enemy.IsDown = false;

            GameEventBus.Publish(new FullResonanceAttackEvent(totalDamage));

            if (CheckBattleEnd())
                ChangeState(AllEnemiesDefeated() ? BattleState.Victory : BattleState.Defeat);
        }

        public void AdvanceTurn()
        {
            IsOneMore = false;
            CurrentTurnIndex = (CurrentTurnIndex + 1) % TurnOrder.Count;

            while (!TurnOrder[CurrentTurnIndex].IsAlive)
            {
                CurrentTurnIndex = (CurrentTurnIndex + 1) % TurnOrder.Count;
            }

            var currentUnit = TurnOrder[CurrentTurnIndex];
            ChangeState(PartyUnits.Contains(currentUnit) ? BattleState.PlayerTurn : BattleState.EnemyTurn);
        }

        public void EndBattle()
        {
            PartyUnits.Clear();
            EnemyUnits.Clear();
            TurnOrder.Clear();
            GameEventBus.Publish(new BattleEndedEvent());
        }

        private void ChangeState(BattleState newState)
        {
            CurrentState = newState;
            OnBattleStateChanged?.Invoke(newState);
        }

        private void CalculateTurnOrder()
        {
            TurnOrder.Clear();
            TurnOrder.AddRange(PartyUnits);
            TurnOrder.AddRange(EnemyUnits);
            TurnOrder.Sort((a, b) => b.Agility.CompareTo(a.Agility));
            CurrentTurnIndex = 0;
        }

        private bool CheckAllEnemiesDown()
        {
            foreach (var enemy in EnemyUnits)
            {
                if (enemy.IsAlive && !enemy.IsDown) return false;
            }
            return true;
        }

        private bool CheckBattleEnd()
        {
            return AllEnemiesDefeated() || AllPartyDefeated();
        }

        private bool AllEnemiesDefeated()
        {
            foreach (var enemy in EnemyUnits)
                if (enemy.IsAlive) return false;
            return true;
        }

        private bool AllPartyDefeated()
        {
            foreach (var member in PartyUnits)
                if (member.IsAlive) return false;
            return true;
        }

        private int CalculateBondBonus()
        {
            // 絆の調べのランク合計に基づくボーナス
            // TODO: SocialLinkManagerから実際のランクを取得
            return 0;
        }

        private int CalculateFullResonanceDamage(BattleUnit target, int bondBonus)
        {
            int baseDamage = 0;
            foreach (var member in PartyUnits)
            {
                if (!member.IsAlive) continue;
                baseDamage += member.Strength + member.Magic;
            }
            return Mathf.Max(1, baseDamage + bondBonus - target.Endurance);
        }
    }

    public readonly struct BattleStartedEvent { }
    public readonly struct BattleEndedEvent { }

    public readonly struct ResonanceEvent
    {
        public readonly BattleUnit Attacker;
        public readonly BattleUnit Target;
        public readonly ElementType Element;

        public ResonanceEvent(BattleUnit attacker, BattleUnit target, ElementType element)
        {
            Attacker = attacker;
            Target = target;
            Element = element;
        }
    }

    public readonly struct AbilityExecutedEvent
    {
        public readonly BattleUnit Attacker;
        public readonly BattleUnit Target;
        public readonly AbilityData Ability;
        public readonly AttackResult Result;

        public AbilityExecutedEvent(BattleUnit attacker, BattleUnit target, AbilityData ability, AttackResult result)
        {
            Attacker = attacker;
            Target = target;
            Ability = ability;
            Result = result;
        }
    }

    public readonly struct FullResonanceAttackEvent
    {
        public readonly int TotalDamage;

        public FullResonanceAttackEvent(int totalDamage)
        {
            TotalDamage = totalDamage;
        }
    }
}
