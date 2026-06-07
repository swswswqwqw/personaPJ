using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Battle;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.UI
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Command Menu")]
        [SerializeField] private CanvasGroup commandMenuGroup;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button abilityButton;
        [SerializeField] private Button guardButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button escapeButton;
        [SerializeField] private RectTransform abilityListPanel;
        [SerializeField] private GameObject abilityButtonPrefab;

        [Header("Party Status")]
        [SerializeField] private PartyMemberStatusUI[] partyStatusSlots;

        [Header("Enemy Display")]
        [SerializeField] private EnemyStatusUI[] enemySlots;

        [Header("Turn Order")]
        [SerializeField] private RectTransform turnOrderBar;
        [SerializeField] private GameObject turnIconPrefab;

        [Header("Battle Messages")]
        [SerializeField] private TextMeshProUGUI battleMessageText;
        [SerializeField] private CanvasGroup messageGroup;
        [SerializeField] private float messageDisplayDuration = 1.5f;

        [Header("Resonance Effect")]
        [SerializeField] private CanvasGroup resonanceOverlay;
        [SerializeField] private Image resonanceWaveImage;
        [SerializeField] private TextMeshProUGUI resonanceText;

        [Header("Full Resonance Attack")]
        [SerializeField] private CanvasGroup fullResonanceOverlay;
        [SerializeField] private TextMeshProUGUI fullResonanceText;
        [SerializeField] private Image[] characterCutinImages;

        [Header("Result Screen")]
        [SerializeField] private CanvasGroup resultOverlay;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI expGainText;
        [SerializeField] private TextMeshProUGUI moneyGainText;

        private BattleUnit selectedTarget;
        private AbilityData selectedAbility;

        private void OnEnable()
        {
            GameEventBus.Subscribe<AbilityExecutedEvent>(OnAbilityExecuted);
            GameEventBus.Subscribe<ResonanceEvent>(OnResonance);
            GameEventBus.Subscribe<FullResonanceAttackEvent>(OnFullResonanceAttack);
            GameEventBus.Subscribe<BattleStartedEvent>(OnBattleStarted);
            GameEventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);

            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;

            attackButton?.onClick.AddListener(OnAttackPressed);
            abilityButton?.onClick.AddListener(OnAbilityMenuPressed);
            guardButton?.onClick.AddListener(OnGuardPressed);
            itemButton?.onClick.AddListener(OnItemPressed);
            escapeButton?.onClick.AddListener(OnEscapePressed);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<AbilityExecutedEvent>(OnAbilityExecuted);
            GameEventBus.Unsubscribe<ResonanceEvent>(OnResonance);
            GameEventBus.Unsubscribe<FullResonanceAttackEvent>(OnFullResonanceAttack);
            GameEventBus.Unsubscribe<BattleStartedEvent>(OnBattleStarted);
            GameEventBus.Unsubscribe<BattleEndedEvent>(OnBattleEnded);

            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;

            attackButton?.onClick.RemoveAllListeners();
            abilityButton?.onClick.RemoveAllListeners();
            guardButton?.onClick.RemoveAllListeners();
            itemButton?.onClick.RemoveAllListeners();
            escapeButton?.onClick.RemoveAllListeners();
        }

        private void OnBattleStarted(BattleStartedEvent e)
        {
            SetOverlayVisible(resonanceOverlay, false);
            SetOverlayVisible(fullResonanceOverlay, false);
            SetOverlayVisible(resultOverlay, false);
            RefreshPartyStatus();
            RefreshEnemyStatus();
        }

        private void OnBattleStateChanged(BattleState state)
        {
            bool showCommands = state == BattleState.PlayerTurn || state == BattleState.ResonanceChance;
            SetOverlayVisible(commandMenuGroup, showCommands);

            if (state == BattleState.ResonanceChance)
                ShowMessage("RESONANCE! もう一度行動できる！");

            if (state == BattleState.Victory)
                ShowVictoryResult();
            else if (state == BattleState.Defeat)
                ShowDefeatResult();
        }

        private void OnAbilityExecuted(AbilityExecutedEvent e)
        {
            RefreshPartyStatus();
            RefreshEnemyStatus();

            string msg;
            if (e.Result.IsMiss)
                msg = $"{e.Attacker.Name}の攻撃は外れた！";
            else if (e.Result.IsNulled)
                msg = $"{e.Target.Name}には効かない！";
            else if (e.Result.IsAbsorbed)
                msg = $"{e.Target.Name}はダメージを吸収した！";
            else if (e.Result.IsReflected)
                msg = $"攻撃が反射された！";
            else
            {
                msg = $"{e.Attacker.Name}の{e.Ability.abilityName}！ {e.Result.Damage}ダメージ！";
                if (e.Result.HitWeakness)
                    msg += " 弱点！";
                if (e.Result.IsCritical)
                    msg += " クリティカル！";
                if (e.Result.IsResisted)
                    msg = $"{e.Attacker.Name}の{e.Ability.abilityName}！ {e.Result.Damage}ダメージ…（耐性）";
            }

            ShowMessage(msg);
        }

        private void OnResonance(ResonanceEvent e)
        {
            ShowResonanceEffect(e.Element);
        }

        private void OnFullResonanceAttack(FullResonanceAttackEvent e)
        {
            ShowFullResonanceAttackEffect(e.TotalDamage);
        }

        private void OnBattleEnded(BattleEndedEvent e)
        {
            SetOverlayVisible(commandMenuGroup, false);
        }

        public void RefreshPartyStatus()
        {
            if (BattleManager.Instance == null) return;
            var party = BattleManager.Instance.PartyUnits;

            for (int i = 0; i < partyStatusSlots.Length; i++)
            {
                if (i < party.Count)
                    partyStatusSlots[i].SetData(party[i]);
                else
                    partyStatusSlots[i].Hide();
            }
        }

        public void RefreshEnemyStatus()
        {
            if (BattleManager.Instance == null) return;
            var enemies = BattleManager.Instance.EnemyUnits;

            for (int i = 0; i < enemySlots.Length; i++)
            {
                if (i < enemies.Count)
                    enemySlots[i].SetData(enemies[i]);
                else
                    enemySlots[i].Hide();
            }
        }

        private async void ShowResonanceEffect(ElementType element)
        {
            if (resonanceOverlay == null) return;

            Color elementColor = UIColors.GetElementColor(element);
            if (resonanceWaveImage != null)
                resonanceWaveImage.color = elementColor;
            if (resonanceText != null)
                resonanceText.text = "RESONANCE";

            SetOverlayVisible(resonanceOverlay, true);
            await System.Threading.Tasks.Task.Delay(800);
            SetOverlayVisible(resonanceOverlay, false);
        }

        private async void ShowFullResonanceAttackEffect(int totalDamage)
        {
            if (fullResonanceOverlay == null) return;

            if (fullResonanceText != null)
                fullResonanceText.text = "FULL RESONANCE ATTACK";

            SetOverlayVisible(fullResonanceOverlay, true);
            await System.Threading.Tasks.Task.Delay(2000);

            if (fullResonanceText != null)
                fullResonanceText.text = $"{totalDamage} DAMAGE!";

            await System.Threading.Tasks.Task.Delay(1500);
            SetOverlayVisible(fullResonanceOverlay, false);
        }

        private async void ShowMessage(string text)
        {
            if (battleMessageText == null) return;
            battleMessageText.text = text;
            SetOverlayVisible(messageGroup, true);
            await System.Threading.Tasks.Task.Delay((int)(messageDisplayDuration * 1000));
            SetOverlayVisible(messageGroup, false);
        }

        private void ShowVictoryResult()
        {
            if (resultOverlay == null) return;
            if (resultTitleText != null) resultTitleText.text = "VICTORY";
            SetOverlayVisible(resultOverlay, true);
        }

        private void ShowDefeatResult()
        {
            if (resultOverlay == null) return;
            if (resultTitleText != null) resultTitleText.text = "DEFEAT";
            SetOverlayVisible(resultOverlay, true);
        }

        private void OnAttackPressed()
        {
            BattleFlowController.Instance?.OnCommandAttack();
        }

        private void OnAbilityMenuPressed()
        {
            BattleFlowController.Instance?.OnCommandAbility();
        }

        private void OnGuardPressed()
        {
            BattleFlowController.Instance?.OnCommandGuard();
        }

        private void OnItemPressed()
        {
            // TODO: アイテムシステム実装後に接続
        }

        private void OnEscapePressed()
        {
            BattleFlowController.Instance?.OnCommandEscape();
        }

        private void SetOverlayVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
