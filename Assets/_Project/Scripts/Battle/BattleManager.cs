using System.Collections.Generic;
using UnityEngine;
using ArcanaOfHollows.Core;
using ArcanaOfHollows.Data;

namespace ArcanaOfHollows.Battle
{
    public enum BattleState
    {
        Initializing,
        PlayerTurn,
        EnemyTurn,
        SelectingAction,
        SelectingTarget,
        ExecutingAction,
        CheckingResult,
        Victory,
        Defeat,
        Escape,
        HarmonyBreak
    }

    public enum ActionResult
    {
        Normal,
        WeaknessHit,
        Critical,
        Blocked,
        Reflected,
        Absorbed,
        Miss
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public BattleState CurrentState { get; private set; }
        public List<BattleUnit> PartyUnits { get; private set; } = new();
        public List<BattleUnit> EnemyUnits { get; private set; } = new();
        public BattleUnit CurrentActor { get; private set; }

        private Queue<BattleUnit> turnOrder = new();
        private int bonusActions;

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
            CurrentState = BattleState.Initializing;
            PartyUnits.Clear();
            EnemyUnits.Clear();
            bonusActions = 0;

            foreach (var member in party)
                PartyUnits.Add(new BattleUnit(member));

            foreach (var enemy in enemies)
                EnemyUnits.Add(new BattleUnit(enemy));

            EventBus.Publish(new BattleStartedEvent(PartyUnits, EnemyUnits));
            CalculateTurnOrder();
            NextTurn();
        }

        public void ExecuteSkill(SkillData skill, BattleUnit target)
        {
            if (CurrentState != BattleState.SelectingTarget) return;

            CurrentState = BattleState.ExecutingAction;
            var result = CalculateDamage(CurrentActor, target, skill);

            EventBus.Publish(new SkillExecutedEvent(CurrentActor, target, skill, result));

            if (result.ActionResult == ActionResult.WeaknessHit ||
                result.ActionResult == ActionResult.Critical)
            {
                bonusActions++;
                EventBus.Publish(new OneMoreEvent(CurrentActor));
            }

            target.CurrentHP -= result.Damage;
            if (target.CurrentHP <= 0)
            {
                target.CurrentHP = 0;
                target.IsDown = true;
            }

            if (result.ActionResult == ActionResult.WeaknessHit)
                target.IsDown = true;

            CheckBattleEnd();
        }

        public bool CanHarmonyBreak()
        {
            return EnemyUnits.TrueForAll(e => e.IsDown || e.CurrentHP <= 0);
        }

        public void TriggerHarmonyBreak()
        {
            if (!CanHarmonyBreak()) return;

            CurrentState = BattleState.HarmonyBreak;
            EventBus.Publish(new HarmonyBreakEvent(PartyUnits));

            foreach (var enemy in EnemyUnits)
            {
                int damage = CalculateHarmonyBreakDamage();
                enemy.CurrentHP -= damage;
                if (enemy.CurrentHP <= 0)
                    enemy.CurrentHP = 0;
            }

            CheckBattleEnd();
        }

        private void NextTurn()
        {
            if (turnOrder.Count == 0)
            {
                CalculateTurnOrder();
            }

            CurrentActor = turnOrder.Dequeue();

            if (CurrentActor.CurrentHP <= 0)
            {
                NextTurn();
                return;
            }

            CurrentActor.IsDown = false;

            bool isParty = PartyUnits.Contains(CurrentActor);
            CurrentState = isParty ? BattleState.PlayerTurn : BattleState.EnemyTurn;
            EventBus.Publish(new TurnStartedEvent(CurrentActor, isParty));

            if (!isParty)
            {
                ExecuteEnemyAI();
            }
        }

        private void CalculateTurnOrder()
        {
            var allUnits = new List<BattleUnit>();
            allUnits.AddRange(PartyUnits);
            allUnits.AddRange(EnemyUnits);
            allUnits.Sort((a, b) => b.Agility.CompareTo(a.Agility));

            turnOrder.Clear();
            foreach (var unit in allUnits)
            {
                if (unit.CurrentHP > 0)
                    turnOrder.Enqueue(unit);
            }
        }

        private DamageResult CalculateDamage(BattleUnit attacker, BattleUnit target, SkillData skill)
        {
            var affinity = target.GetAffinity(skill.Element);

            if (affinity == ElementAffinity.Null)
                return new DamageResult(0, ActionResult.Blocked);
            if (affinity == ElementAffinity.Reflect)
                return new DamageResult(skill.BasePower / 2, ActionResult.Reflected);
            if (affinity == ElementAffinity.Absorb)
                return new DamageResult(-skill.BasePower / 2, ActionResult.Absorbed);

            float multiplier = affinity switch
            {
                ElementAffinity.Weak => 1.5f,
                ElementAffinity.Resist => 0.5f,
                _ => 1.0f
            };

            int baseDamage = skill.IsPhysical
                ? attacker.Strength * skill.BasePower / 10
                : attacker.Magic * skill.BasePower / 10;

            int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
            var result = affinity == ElementAffinity.Weak ? ActionResult.WeaknessHit : ActionResult.Normal;

            if (Random.value < attacker.LuckCritRate)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * 1.5f);
                result = ActionResult.Critical;
            }

            return new DamageResult(finalDamage, result);
        }

        private int CalculateHarmonyBreakDamage()
        {
            int totalPower = 0;
            foreach (var unit in PartyUnits)
            {
                if (unit.CurrentHP > 0)
                    totalPower += (unit.Strength + unit.Magic) / 2;
            }
            return totalPower * 3;
        }

        private void ExecuteEnemyAI()
        {
            if (PartyUnits.Count == 0) return;

            var target = PartyUnits[Random.Range(0, PartyUnits.Count)];
            while (target.CurrentHP <= 0 && PartyUnits.Exists(u => u.CurrentHP > 0))
                target = PartyUnits[Random.Range(0, PartyUnits.Count)];

            var skill = CurrentActor.GetRandomSkill();
            if (skill != null)
                ExecuteSkill(skill, target);
            else
                NextTurn();
        }

        private void CheckBattleEnd()
        {
            if (EnemyUnits.TrueForAll(e => e.CurrentHP <= 0))
            {
                CurrentState = BattleState.Victory;
                EventBus.Publish(new BattleEndedEvent(true));
                return;
            }

            if (PartyUnits.TrueForAll(p => p.CurrentHP <= 0))
            {
                CurrentState = BattleState.Defeat;
                EventBus.Publish(new BattleEndedEvent(false));
                return;
            }

            if (bonusActions > 0)
            {
                bonusActions--;
                CurrentState = BattleState.PlayerTurn;
                EventBus.Publish(new TurnStartedEvent(CurrentActor, true));
            }
            else
            {
                NextTurn();
            }
        }
    }

    public class BattleUnit
    {
        public string Name { get; }
        public int MaxHP { get; }
        public int CurrentHP { get; set; }
        public int MaxSP { get; }
        public int CurrentSP { get; set; }
        public int Strength { get; }
        public int Magic { get; }
        public int Endurance { get; }
        public int Agility { get; }
        public int Luck { get; }
        public bool IsDown { get; set; }
        public float LuckCritRate => Luck / 100f * 0.15f;

        private readonly ElementAffinity[] affinities;
        private readonly List<SkillData> skills;

        public BattleUnit(CharacterData data)
        {
            Name = data.characterName;
            MaxHP = data.maxHP;
            CurrentHP = data.maxHP;
            MaxSP = data.maxSP;
            CurrentSP = data.maxSP;
            Strength = data.strength;
            Magic = data.magic;
            Endurance = data.endurance;
            Agility = data.agility;
            Luck = data.luck;
            affinities = data.elementAffinities;
            skills = new List<SkillData>(data.skills);
        }

        public BattleUnit(EnemyData data)
        {
            Name = data.enemyName;
            MaxHP = data.maxHP;
            CurrentHP = data.maxHP;
            MaxSP = data.maxSP;
            CurrentSP = data.maxSP;
            Strength = data.strength;
            Magic = data.magic;
            Endurance = data.endurance;
            Agility = data.agility;
            Luck = data.luck;
            affinities = data.elementAffinities;
            skills = new List<SkillData>(data.skills);
        }

        public ElementAffinity GetAffinity(Element element)
        {
            int index = (int)element;
            if (index < 0 || index >= affinities.Length)
                return ElementAffinity.Normal;
            return affinities[index];
        }

        public SkillData GetRandomSkill()
        {
            if (skills.Count == 0) return null;
            return skills[Random.Range(0, skills.Count)];
        }
    }

    public readonly struct DamageResult
    {
        public readonly int Damage;
        public readonly ActionResult ActionResult;

        public DamageResult(int damage, ActionResult result)
        {
            Damage = damage;
            ActionResult = result;
        }
    }

    public readonly struct BattleStartedEvent
    {
        public readonly List<BattleUnit> Party;
        public readonly List<BattleUnit> Enemies;

        public BattleStartedEvent(List<BattleUnit> party, List<BattleUnit> enemies)
        {
            Party = party;
            Enemies = enemies;
        }
    }

    public readonly struct BattleEndedEvent
    {
        public readonly bool IsVictory;
        public BattleEndedEvent(bool isVictory) { IsVictory = isVictory; }
    }

    public readonly struct TurnStartedEvent
    {
        public readonly BattleUnit Actor;
        public readonly bool IsPartyMember;

        public TurnStartedEvent(BattleUnit actor, bool isPartyMember)
        {
            Actor = actor;
            IsPartyMember = isPartyMember;
        }
    }

    public readonly struct SkillExecutedEvent
    {
        public readonly BattleUnit Attacker;
        public readonly BattleUnit Target;
        public readonly SkillData Skill;
        public readonly DamageResult Result;

        public SkillExecutedEvent(BattleUnit attacker, BattleUnit target, SkillData skill, DamageResult result)
        {
            Attacker = attacker;
            Target = target;
            Skill = skill;
            Result = result;
        }
    }

    public readonly struct OneMoreEvent
    {
        public readonly BattleUnit Actor;
        public OneMoreEvent(BattleUnit actor) { Actor = actor; }
    }

    public readonly struct HarmonyBreakEvent
    {
        public readonly List<BattleUnit> Party;
        public HarmonyBreakEvent(List<BattleUnit> party) { Party = party; }
    }
}
