using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Amane.Battle;
using Amane.Core;
using Amane.Echo;
using Amane.UI.Effects;

namespace Amane.UI
{
    public sealed class BattleController : MonoBehaviour
    {
        [SerializeField] private BattleHUD _hud;
        [SerializeField] private SkillSelectUI _skillSelect;
        [SerializeField] private TargetSelectUI _targetSelect;
        [SerializeField] private KotsugiSelectUI _kotsugiSelect;
        [SerializeField] private BattleResultUI _resultUI;
        [SerializeField] private BattleEffects _effects;
        [SerializeField] private ActionCommand _actionCommand;
        [SerializeField] private Effects.LevelUpEffect _levelUpEffect;

        private BattleManager _battle;
        private bool _playerInputNeeded;
        private Skill _pendingSkill;
        private List<Combatant> _pendingTargets;

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            _battle = new BattleManager(gm.Events);

            if (_hud != null)
            {
                _hud.Bind(_battle);
                _hud.OnAttackPressed += OnAttack;
                _hud.OnSkillPressed += OnSkillMenu;
                _hud.OnGuardPressed += OnGuard;
                _hud.OnKotsugiPressed += OnKotsugiMenu;
                _hud.OnAllOutPressed += OnAllOut;
                _hud.OnFleePressed += OnFlee;
            }

            if (_skillSelect != null)
            {
                _skillSelect.OnSkillSelected += OnSkillChosen;
                _skillSelect.OnBack += ShowCommandPanel;
            }

            if (_targetSelect != null)
            {
                _targetSelect.OnTargetSelected += OnTargetChosen;
                _targetSelect.OnBack += ShowCommandPanel;
            }

            if (_kotsugiSelect != null)
            {
                _kotsugiSelect.OnMemberSelected += OnKotsugiTargetChosen;
                _kotsugiSelect.OnBack += ShowCommandPanel;
            }

            if (_resultUI != null)
                _resultUI.OnContinue += OnResultContinue;

            _battle.OnPhaseChanged += OnPhaseChanged;
            _battle.OnHit += OnHitResult;
            _battle.OnAllOutConfession += OnAllOutConfessionTriggered;
            _battle.OnKotsugi += OnKotsugiTriggered;
            _battle.OnPerfectKotsugi += OnPerfectKotsugiTriggered;
            _battle.OnReverseAllOutCalling += OnReverseAllOutCallingTriggered;

            StartBattle();
        }

        private void OnDisable()
        {
            if (_battle != null)
            {
                _battle.OnPhaseChanged -= OnPhaseChanged;
                _battle.OnHit -= OnHitResult;
                _battle.OnAllOutConfession -= OnAllOutConfessionTriggered;
                _battle.OnKotsugi -= OnKotsugiTriggered;
                _battle.OnPerfectKotsugi -= OnPerfectKotsugiTriggered;
                _battle.OnReverseAllOutCalling -= OnReverseAllOutCallingTriggered;
            }
        }

        private void StartBattle()
        {
            var party = PartyFactory.CreateDefaultParty();
            var enemies = EnemyFactory.CreateTutorialEncounter();
            _battle.StartBattle(party, enemies);
            _hud?.RefreshAll();
        }

        // ===== Battle Events → Visual Effects =====

        private void OnHitResult(HitResult result)
        {
            if (_effects == null) return;

            // ダメージポップ（ダメージが発生した場合）
            if (result.Damage > 0)
            {
                // 敵の位置をインデックスから推定（右側に並ぶ）
                int idx = _battle.Enemies.IndexOf(result.Target);
                if (idx < 0) idx = _battle.Party.IndexOf(result.Target);
                Vector2 popPos = new Vector2(100 + idx * 60, UnityEngine.Random.Range(-20f, 20f));
                _effects.PlayDamageNumber(result.Damage, result.Type, popPos);
            }

            // ヒットタイプに応じた演出
            switch (result.Type)
            {
                case HitType.Weak:
                    _effects.PlayWeakHit();
                    if (result.CausedDown) _effects.PlayDown();
                    _hud?.Log($"弱点ヒット！ {result.Target.DisplayName}に{result.Damage}ダメージ！");
                    break;

                case HitType.Critical:
                    _effects.PlayCriticalHit();
                    if (result.CausedDown) _effects.PlayDown();
                    _hud?.Log($"クリティカル！ {result.Target.DisplayName}に{result.Damage}ダメージ！");
                    break;

                case HitType.Null:
                    _effects.PlayNull();
                    _hud?.Log($"{result.Target.DisplayName}には効かなかった！");
                    break;

                case HitType.Drain:
                    _hud?.Log($"{result.Target.DisplayName}は攻撃を吸収した！");
                    break;

                case HitType.Repel:
                    _hud?.Log($"{result.Target.DisplayName}は攻撃を反射した！");
                    break;

                case HitType.Resist:
                    _hud?.Log($"{result.Target.DisplayName}に{result.Damage}ダメージ（耐性）");
                    break;

                case HitType.Miss:
                    _hud?.Log($"{result.Target.DisplayName}に外れた！");
                    break;

                default:
                    _hud?.Log($"{result.Target.DisplayName}に{result.Damage}ダメージ");
                    break;
            }

            _hud?.RefreshAll();
        }

        private void OnAllOutConfessionTriggered()
        {
            if (_effects == null) return;

            // フィニッシャーは主人公
            string finisher = "天野 詠";
            string[] quotes = {
                "伝えられなかった想い、今ここで全部叫ぶ。",
                "届かなくても、声にする。それが私の戦い。",
                "残響は消えない。この言葉は、永遠に。"
            };
            string quote = quotes[UnityEngine.Random.Range(0, quotes.Length)];
            _effects.PlayAllOutConfession(finisher, quote);
            _hud?.Log("── 総告白 ──");
        }

        private void OnKotsugiTriggered(Combatant from, Combatant to, float bonus)
        {
            if (_effects != null)
                _effects.PlayKotsugi(from.DisplayName, to.DisplayName, bonus);

            _hud?.Log($"言継ぎ！ {from.DisplayName} → {to.DisplayName}（ダメージ+{bonus:P0}）");
            _hud?.RefreshAll();
        }

        private void OnPerfectKotsugiTriggered()
        {
            _effects?.PlayPerfectKotsugi();
            _hud?.Log("── パーフェクト言継ぎ ── SP回復＋次ターン攻撃力+10%！");
            _hud?.RefreshAll();
        }

        private void OnReverseAllOutCallingTriggered(Combatant enemy)
        {
            _effects?.PlayReverseAllOutCalling(enemy.DisplayName);
            _hud?.Log($"── 逆総告白 ── {enemy.DisplayName}の連続攻撃！");
            _hud?.RefreshAll();
        }

        // ===== Phase Changes =====

        private void OnPhaseChanged(BattlePhase phase)
        {
            switch (phase)
            {
                case BattlePhase.PlayerTurn:
                    _playerInputNeeded = true;
                    ShowCommandPanel();
                    _hud?.Log($"{_battle.ActiveCombatant.DisplayName}のターン");
                    break;

                case BattlePhase.EnemyTurn:
                    _playerInputNeeded = false;
                    HideAllPanels();
                    ExecuteEnemyTurn();
                    break;

                case BattlePhase.Victory:
                    _playerInputNeeded = false;
                    HideAllPanels();
                    HandleVictory();
                    break;

                case BattlePhase.Defeat:
                    _playerInputNeeded = false;
                    HideAllPanels();
                    _resultUI?.ShowDefeat();
                    break;

                case BattlePhase.Fled:
                    _playerInputNeeded = false;
                    HideAllPanels();
                    ReturnToField();
                    break;
            }
        }

        private void ShowCommandPanel()
        {
            _skillSelect?.Hide();
            _targetSelect?.Hide();
            _kotsugiSelect?.Hide();

            bool canKotsugi = _battle.KotsugiChain >= 0 &&
                              _battle.Party.Count(p => p.IsAlive && p != _battle.ActiveCombatant) > 0;
            bool canAllOut = _battle.CanAllOutConfession();
            _hud?.ShowCommands(canKotsugi, canAllOut);
        }

        private void HideAllPanels()
        {
            _hud?.HideCommands();
            _skillSelect?.Hide();
            _targetSelect?.Hide();
            _kotsugiSelect?.Hide();
        }

        // ===== Player Input =====

        private void OnAttack()
        {
            if (!_playerInputNeeded) return;
            _pendingSkill = Skill.MeleeAttack;
            _hud?.HideCommands();
            _targetSelect?.Show(_battle.Enemies.Where(e => e.IsAlive).ToList());
        }

        private void OnSkillMenu()
        {
            if (!_playerInputNeeded) return;
            _hud?.HideCommands();
            _skillSelect?.Show(_battle.ActiveCombatant);
        }

        private void OnSkillChosen(Skill skill)
        {
            _pendingSkill = skill;
            if (skill.Target == TargetType.AllEnemies)
            {
                ExecuteSkillOnTargets(_battle.Enemies.Where(e => e.IsAlive).ToList());
            }
            else if (skill.Target == TargetType.SingleAlly || skill.Target == TargetType.AllAllies)
            {
                var targets = skill.Target == TargetType.AllAllies
                    ? _battle.Party.Where(p => p.IsAlive).ToList()
                    : null;
                if (targets != null)
                    ExecuteSkillOnTargets(targets);
                else
                    _targetSelect?.Show(_battle.Party.Where(p => p.IsAlive).ToList());
            }
            else if (skill.Target == TargetType.Self)
            {
                ExecuteSkillOnTargets(new List<Combatant> { _battle.ActiveCombatant });
            }
            else
            {
                _targetSelect?.Show(_battle.Enemies.Where(e => e.IsAlive).ToList());
            }
        }

        private void OnTargetChosen(Combatant target)
        {
            ExecuteSkillOnTargets(new List<Combatant> { target });
        }

        private void ExecuteSkillOnTargets(List<Combatant> targets)
        {
            _playerInputNeeded = false;
            HideAllPanels();

            // アクションコマンド: プレイヤーのスキル攻撃時のみタイミング入力
            if (_actionCommand != null && _battle.ActiveCombatant.IsPlayer && _pendingSkill.BasePower > 0)
            {
                _pendingTargets = targets;
                _actionCommand.StartCommand(multiplier =>
                {
                    // ダメージ倍率を一時的にスキルに反映
                    var skill = _pendingSkill;
                    if (multiplier > 1f)
                    {
                        // 倍率付きスキルを一時生成
                        skill = new Skill(
                            skill.Id + "_boosted", skill.DisplayName, skill.Element,
                            (int)(skill.BasePower * multiplier), skill.SpCost,
                            skill.Target, skill.IsPhysical, skill.CritRate);
                    }
                    var action = BattleAction.UseSkill(_battle.ActiveCombatant, skill, _pendingTargets);
                    _battle.ExecuteAction(action);
                    _pendingTargets = null;
                });
            }
            else
            {
                var action = BattleAction.UseSkill(_battle.ActiveCombatant, _pendingSkill, targets);
                _battle.ExecuteAction(action);
            }
        }

        private void OnGuard()
        {
            if (!_playerInputNeeded) return;
            _playerInputNeeded = false;
            HideAllPanels();
            _battle.ExecuteAction(BattleAction.Guard(_battle.ActiveCombatant));
        }

        private void OnKotsugiMenu()
        {
            if (!_playerInputNeeded) return;
            _hud?.HideCommands();
            _kotsugiSelect?.Show(_battle.Party, _battle.ActiveCombatant);
        }

        private void OnKotsugiTargetChosen(Combatant receiver)
        {
            _playerInputNeeded = false;
            HideAllPanels();
            _battle.ExecuteAction(BattleAction.Kotsugi(_battle.ActiveCombatant, receiver));
        }

        private void OnAllOut()
        {
            if (!_playerInputNeeded || !_battle.CanAllOutConfession()) return;
            _playerInputNeeded = false;
            HideAllPanels();
            _battle.TriggerAllOutConfession();
        }

        private void OnFlee()
        {
            if (!_playerInputNeeded) return;
            _playerInputNeeded = false;
            HideAllPanels();
            _battle.ExecuteAction(BattleAction.Flee(_battle.ActiveCombatant));
        }

        private void ExecuteEnemyTurn()
        {
            var enemy = _battle.ActiveCombatant;
            if (enemy == null || !enemy.IsAlive) return;
            var action = EnemyAI.DecideAction(enemy, _battle.Party);
            _battle.ExecuteAction(action);
        }

        private void HandleVictory()
        {
            var gm = GameManager.Instance;
            int exp = ExperienceSystem.CalculateBattleExp(_battle.Enemies);

            // EXP付与＋レベルアップ判定
            var allLevelUps = new System.Collections.Generic.List<LevelUpResult>();
            if (gm?.Experience != null)
            {
                foreach (var member in _battle.Party)
                {
                    var levelUps = gm.Experience.GiveExp(member.Id, exp);
                    allLevelUps.AddRange(levelUps);
                }
            }

            // SE
            var audio = Core.Audio.AudioManager.Instance;
            if (audio != null) audio.PlayImpact();

            // レベルアップ演出があれば先に再生、なければ直接リザルト
            if (_levelUpEffect != null && allLevelUps.Count > 0)
            {
                string leaderId = _battle.Party[0].Id;
                int newLevel = gm?.Experience?.GetLevel(leaderId) ?? 1;
                float progress = gm?.Experience?.GetExpProgress(leaderId) ?? 0;

                _levelUpEffect.Play(exp, newLevel, progress, allLevelUps, () =>
                {
                    _resultUI?.ShowVictory(exp);
                });
            }
            else
            {
                _resultUI?.ShowVictory(exp);
            }
        }

        private void OnResultContinue()
        {
            if (DungeonBattleContext.IsInDungeon)
                ReturnToDungeon();
            else
                ReturnToField();
        }

        private void ReturnToField()
        {
            var transition = TransitionEffect.Instance;
            if (transition != null)
                transition.PlayReturnTransition(() => GameManager.Instance?.ReturnToField());
            else
                GameManager.Instance?.ReturnToField();
        }

        private void ReturnToDungeon()
        {
            DungeonBattleContext.BattleWon = true;
            var transition = TransitionEffect.Instance;
            if (transition != null)
                transition.PlayReturnTransition(() => GameManager.Instance?.ReturnToDungeon());
            else
                GameManager.Instance?.ReturnToDungeon();
        }
    }
}
