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
        private readonly Random _rng = new();

        // パーフェクト言継ぎ追跡
        private readonly HashSet<string> _kotsugiChainParticipants = new();
        private bool _perfectKotsugiBuffNextRound = false;
        private bool _perfectKotsugiBuffThisRound = false;

        public List<Combatant> Party { get; } = new();
        public List<Combatant> Enemies { get; } = new();
        public BattlePhase Phase { get; private set; }
        public Combatant ActiveCombatant => _turns.Current;
        public int KotsugiChain { get; private set; }
        public bool PerfectKotsugiBuffActive => _perfectKotsugiBuffThisRound;

        public event Action<BattlePhase> OnPhaseChanged;
        public event Action<HitResult> OnHit;
        public event Action OnAllOutConfession;
        public event Action<Combatant, Combatant, float> OnKotsugi;
        // DESIGN.md 9-1: パーフェクト言継ぎ — 4人全員リレー達成時
        public event Action OnPerfectKotsugi;
        // DESIGN.md 9-1: 逆総告白 — 味方全員DOWN時に敵が連続行動
        public event Action<Combatant> OnReverseAllOutCalling;

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
            _kotsugiChainParticipants.Clear();
            _perfectKotsugiBuffNextRound = false;
            _perfectKotsugiBuffThisRound = false;

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

        // パーフェクト言継ぎバフ（+10%）を加算した合計ボーナスを返す
        public float GetTotalBonus() => GetKotsugiBonus() + (_perfectKotsugiBuffThisRound ? 0.1f : 0f);

        private void ExecuteSkill(BattleAction action)
        {
            var skill = action.Skill ?? Skill.MeleeAttack;
            if (!action.Actor.SpendSp(skill.SpCost))
                skill = Skill.MeleeAttack;

            float bonus = GetTotalBonus();
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
                _kotsugiChainParticipants.Clear();
            }

            // 逆総告白チェック: 敵の攻撃で味方全員がDOWNになったか
            if (!action.Actor.IsPlayer)
                CheckReverseAllOutCalling(action.Actor);
        }

        private void ExecuteKotsugi(BattleAction action)
        {
            var receiver = action.Targets?.FirstOrDefault();
            if (receiver == null || !receiver.IsAlive) return;

            KotsugiChain++;
            // 参加者を記録
            _kotsugiChainParticipants.Add(action.Actor.Id);
            _kotsugiChainParticipants.Add(receiver.Id);

            float bonus = GetTotalBonus();
            OnKotsugi?.Invoke(action.Actor, receiver, bonus);
            _turns.InsertOneMore(receiver);

            // パーフェクト言継ぎチェック
            CheckPerfectKotsugi();

            AdvanceTurn();
        }

        // DESIGN.md 9-1: 4人全員を1ターン内でリレーしたか判定
        private void CheckPerfectKotsugi()
        {
            var aliveParty = Party.Where(p => p.IsAlive).ToList();
            // 2人以上生存 && 全員が参加した
            if (aliveParty.Count < 2) return;
            if (!aliveParty.All(p => _kotsugiChainParticipants.Contains(p.Id))) return;

            OnPerfectKotsugi?.Invoke();

            // 即時: SP全員15%回復
            foreach (var member in aliveParty)
                member.RestoreSp((int)(member.MaxSp * 0.15f));

            // 次ラウンドバフを予約
            _perfectKotsugiBuffNextRound = true;
        }

        // DESIGN.md 9-1: 味方全員DOWN → 敵が連続行動
        private void CheckReverseAllOutCalling(Combatant enemy)
        {
            var aliveParty = Party.Where(p => p.IsAlive).ToList();
            if (aliveParty.Count == 0) return;
            if (!aliveParty.All(p => p.IsDown)) return;

            OnReverseAllOutCalling?.Invoke(enemy);

            // 2〜3回の追撃
            int extraCount = 2 + _rng.Next(2);
            for (int i = 0; i < extraCount; i++)
            {
                aliveParty = Party.Where(p => p.IsAlive).ToList();
                if (aliveParty.Count == 0) break;
                var target = aliveParty[_rng.Next(aliveParty.Count)];
                var result = DamageCalculator.Calculate(enemy, target, Skill.MeleeAttack, 0f);
                OnHit?.Invoke(result);
            }

            CheckBattleEnd();
        }

        private void BeginNewRound()
        {
            // パーフェクト言継ぎバフをローテーション（次ラウンド予約→今ラウンド適用）
            _perfectKotsugiBuffThisRound = _perfectKotsugiBuffNextRound;
            _perfectKotsugiBuffNextRound = false;
            _kotsugiChainParticipants.Clear();

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
