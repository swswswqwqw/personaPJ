using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amane.Battle;
using Amane.Core;

namespace Amane.UI
{
    public sealed class BattleHUD : MonoBehaviour
    {
        [Header("Party Status")]
        [SerializeField] private Text[] _partyNames;
        [SerializeField] private Text[] _partyHpTexts;
        [SerializeField] private Text[] _partySpTexts;

        [Header("Enemy Status")]
        [SerializeField] private Text[] _enemyNames;
        [SerializeField] private Text[] _enemyHpTexts;

        [Header("Command")]
        [SerializeField] private GameObject _commandPanel;
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _skillButton;
        [SerializeField] private Button _guardButton;
        [SerializeField] private Button _kotsugiButton;
        [SerializeField] private Button _allOutButton;
        [SerializeField] private Button _fleeButton;

        [Header("Log")]
        [SerializeField] private Text _logText;

        private BattleManager _battle;

        public event Action OnAttackPressed;
        public event Action OnSkillPressed;
        public event Action OnGuardPressed;
        public event Action OnKotsugiPressed;
        public event Action OnAllOutPressed;
        public event Action OnFleePressed;

        private void Awake()
        {
            _attackButton?.onClick.AddListener(() => OnAttackPressed?.Invoke());
            _skillButton?.onClick.AddListener(() => OnSkillPressed?.Invoke());
            _guardButton?.onClick.AddListener(() => OnGuardPressed?.Invoke());
            _kotsugiButton?.onClick.AddListener(() => OnKotsugiPressed?.Invoke());
            _allOutButton?.onClick.AddListener(() => OnAllOutPressed?.Invoke());
            _fleeButton?.onClick.AddListener(() => OnFleePressed?.Invoke());
        }

        public void Bind(BattleManager battle)
        {
            _battle = battle;
            _battle.OnHit += OnHit;
            _battle.OnPhaseChanged += OnPhaseChanged;
            _battle.OnAllOutConfession += OnAllOut;
            _battle.OnKotsugi += OnKotsugiExec;
        }

        public void RefreshAll()
        {
            if (_battle == null) return;
            RefreshParty(_battle.Party);
            RefreshEnemies(_battle.Enemies);
        }

        public void ShowCommands(bool canKotsugi, bool canAllOut)
        {
            if (_commandPanel != null) _commandPanel.SetActive(true);
            SetInteractable(_kotsugiButton, canKotsugi);
            SetInteractable(_allOutButton, canAllOut);
        }

        public void HideCommands()
        {
            if (_commandPanel != null) _commandPanel.SetActive(false);
        }

        public void Log(string message)
        {
            if (_logText != null)
                _logText.text = message + "\n" + (_logText.text.Length > 500 ? _logText.text[..500] : _logText.text);
        }

        private void RefreshParty(List<Combatant> party)
        {
            for (int i = 0; i < _partyNames.Length; i++)
            {
                if (i < party.Count)
                {
                    SetText(_partyNames[i], party[i].DisplayName);
                    SetText(_partyHpTexts[i], $"HP {party[i].Hp}/{party[i].MaxHp}");
                    SetText(_partySpTexts[i], $"SP {party[i].Sp}/{party[i].MaxSp}");
                }
                else
                {
                    SetText(_partyNames[i], "");
                    SetText(_partyHpTexts[i], "");
                    SetText(_partySpTexts[i], "");
                }
            }
        }

        private void RefreshEnemies(List<Combatant> enemies)
        {
            for (int i = 0; i < _enemyNames.Length; i++)
            {
                if (i < enemies.Count && enemies[i].IsAlive)
                {
                    SetText(_enemyNames[i], enemies[i].DisplayName);
                    SetText(_enemyHpTexts[i], $"HP {enemies[i].Hp}/{enemies[i].MaxHp}");
                }
                else
                {
                    SetText(_enemyNames[i], "");
                    SetText(_enemyHpTexts[i], "");
                }
            }
        }

        private void OnHit(HitResult r)
        {
            string msg = r.Type switch
            {
                HitType.Weak => $"WEAK! {r.Target.DisplayName}に{r.Damage}ダメージ！",
                HitType.Critical => $"CRITICAL! {r.Target.DisplayName}に{r.Damage}ダメージ！",
                HitType.Null => $"{r.Target.DisplayName}に効かなかった！",
                HitType.Drain => $"{r.Target.DisplayName}が吸収した！",
                HitType.Repel => $"反射された！",
                HitType.Resist => $"{r.Target.DisplayName}に{r.Damage}ダメージ（耐性）",
                HitType.Miss => $"ミス！",
                _ => $"{r.Target.DisplayName}に{r.Damage}ダメージ"
            };
            if (r.CausedDown) msg += " DOWN!";
            Log(msg);
            RefreshAll();
        }

        private void OnPhaseChanged(BattlePhase phase)
        {
            switch (phase)
            {
                case BattlePhase.Victory:
                    Log("――勝利。澱は静まった。");
                    HideCommands();
                    break;
                case BattlePhase.Defeat:
                    Log("――意識が、遠のいていく……");
                    HideCommands();
                    break;
            }
        }

        private void OnAllOut()
        {
            Log("【総告白（ALL-OUT CONFESSION）】——言えなかった言葉が、いま、すべてを貫く！");
        }

        private void OnKotsugiExec(Combatant from, Combatant to, float bonus)
        {
            Log($"【言継ぎ】{from.DisplayName}→{to.DisplayName}！ （ダメージ+{bonus:P0}）");
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
        private static void SetInteractable(Button b, bool v) { if (b != null) b.interactable = v; }
    }
}
