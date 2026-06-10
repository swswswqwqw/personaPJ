using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfEchoes.Core;
using AriaOfEchoes.Data;

namespace AriaOfEchoes.Battle
{
    public enum BattleState
    {
        Init,
        TurnStart,
        PlayerCommand,
        PlayerAction,
        EnemyAction,
        TurnEnd,
        Victory,
        Defeat,
        Escape
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        BattleState state;
        List<BattleUnit> turnOrder = new();
        int currentUnitIndex;
        bool echoGranted; // ワンモア・エコー（追加ターン）

        public BattleState State => state;
        public BattleUnit CurrentUnit => turnOrder[currentUnitIndex];
        public List<BattleUnit> PlayerUnits { get; } = new();
        public List<BattleUnit> EnemyUnits { get; } = new();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartBattle(List<BattleUnit> party, List<BattleUnit> enemies)
        {
            PlayerUnits.Clear();
            PlayerUnits.AddRange(party);
            EnemyUnits.Clear();
            EnemyUnits.AddRange(enemies);

            CalculateTurnOrder();
            currentUnitIndex = 0;
            echoGranted = false;

            ChangeState(BattleState.Init);
            EventBus.Publish(new BattleStartEvent(party, enemies));
            ChangeState(BattleState.TurnStart);
        }

        public void ExecuteSkill(BattleUnit attacker, SkillData skill, BattleUnit target)
        {
            var result = DamageCalculator.Calculate(attacker, skill, target);

            target.ApplyDamage(result.damage);
            EventBus.Publish(new SkillExecutedEvent(attacker, skill, target, result));

            if (result.affinityResult == AffinityType.Weak && !target.IsDown)
            {
                target.SetDown(true);
                echoGranted = true;
                EventBus.Publish(new WeaknessHitEvent(attacker, target));
            }
            else if (result.affinityResult == AffinityType.Reflect)
            {
                attacker.ApplyDamage(result.damage);
                EventBus.Publish(new ReflectEvent(attacker, target));
            }

            if (result.isCritical && !target.IsDown)
            {
                target.SetDown(true);
                echoGranted = true;
            }

            CheckBattleEnd();
        }

        public bool CanUnisonBurst()
        {
            return EnemyUnits.TrueForAll(e => e.IsDown || e.IsDead);
        }

        public void ExecuteUnisonBurst()
        {
            if (!CanUnisonBurst()) return;

            int totalDamage = 0;
            foreach (var ally in PlayerUnits)
            {
                if (!ally.IsDead)
                    totalDamage += ally.Stats.Strength + ally.Stats.Magic;
            }

            foreach (var enemy in EnemyUnits)
            {
                if (!enemy.IsDead)
                    enemy.ApplyDamage(totalDamage);
            }

            EventBus.Publish(new UnisonBurstEvent(PlayerUnits, EnemyUnits, totalDamage));
            CheckBattleEnd();
        }

        public void AdvanceTurn()
        {
            if (echoGranted)
            {
                echoGranted = false;
                EventBus.Publish(new EchoTurnEvent(CurrentUnit));
                return;
            }

            currentUnitIndex++;
            if (currentUnitIndex >= turnOrder.Count)
            {
                currentUnitIndex = 0;
                CalculateTurnOrder();
                ChangeState(BattleState.TurnEnd);
                ChangeState(BattleState.TurnStart);
                return;
            }

            var unit = CurrentUnit;
            if (unit.IsDead)
            {
                AdvanceTurn();
                return;
            }

            ChangeState(unit.IsPlayerControlled
                ? BattleState.PlayerCommand
                : BattleState.EnemyAction);
        }

        void CalculateTurnOrder()
        {
            turnOrder.Clear();
            turnOrder.AddRange(PlayerUnits);
            turnOrder.AddRange(EnemyUnits);
            turnOrder.Sort((a, b) => b.Stats.Agility.CompareTo(a.Stats.Agility));
        }

        void CheckBattleEnd()
        {
            if (EnemyUnits.TrueForAll(e => e.IsDead))
            {
                ChangeState(BattleState.Victory);
                EventBus.Publish(new BattleEndEvent(true));
            }
            else if (PlayerUnits.TrueForAll(p => p.IsDead))
            {
                ChangeState(BattleState.Defeat);
                EventBus.Publish(new BattleEndEvent(false));
            }
        }

        void ChangeState(BattleState newState)
        {
            state = newState;
            EventBus.Publish(new BattleStateChangedEvent(newState));
        }
    }

    public struct BattleStartEvent
    {
        public List<BattleUnit> Party;
        public List<BattleUnit> Enemies;
        public BattleStartEvent(List<BattleUnit> party, List<BattleUnit> enemies)
        { Party = party; Enemies = enemies; }
    }

    public struct BattleEndEvent
    {
        public bool IsVictory;
        public BattleEndEvent(bool isVictory) { IsVictory = isVictory; }
    }

    public struct SkillExecutedEvent
    {
        public BattleUnit Attacker;
        public SkillData Skill;
        public BattleUnit Target;
        public DamageResult Result;
        public SkillExecutedEvent(BattleUnit a, SkillData s, BattleUnit t, DamageResult r)
        { Attacker = a; Skill = s; Target = t; Result = r; }
    }

    public struct WeaknessHitEvent
    {
        public BattleUnit Attacker;
        public BattleUnit Target;
        public WeaknessHitEvent(BattleUnit a, BattleUnit t)
        { Attacker = a; Target = t; }
    }

    public struct ReflectEvent
    {
        public BattleUnit Attacker;
        public BattleUnit Target;
        public ReflectEvent(BattleUnit a, BattleUnit t)
        { Attacker = a; Target = t; }
    }

    public struct UnisonBurstEvent
    {
        public List<BattleUnit> Allies;
        public List<BattleUnit> Enemies;
        public int TotalDamage;
        public UnisonBurstEvent(List<BattleUnit> a, List<BattleUnit> e, int d)
        { Allies = a; Enemies = e; TotalDamage = d; }
    }

    public struct EchoTurnEvent
    {
        public BattleUnit Unit;
        public EchoTurnEvent(BattleUnit u) { Unit = u; }
    }

    public struct BattleStateChangedEvent
    {
        public BattleState NewState;
        public BattleStateChangedEvent(BattleState s) { NewState = s; }
    }
}
