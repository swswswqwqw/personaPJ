using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
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
        [SerializeField] private RectTransform resultPanel;
        [SerializeField] private TextMeshProUGUI resultTitleText;
        [SerializeField] private TextMeshProUGUI expGainText;
        [SerializeField] private TextMeshProUGUI moneyGainText;

        [Header("Damage Popup")]
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField] private RectTransform damagePopupContainer;

        [Header("Animation Targets")]
        [SerializeField] private RectTransform commandMenuRect;
        [SerializeField] private RectTransform messageRect;

        private BattleUnit selectedTarget;
        private AbilityData selectedAbility;
        private Tween activeMsgTween;

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
            UIAnimator.SetVisible(resonanceOverlay, false);
            UIAnimator.SetVisible(fullResonanceOverlay, false);
            UIAnimator.SetVisible(resultOverlay, false);
            RefreshPartyStatus();
            RefreshEnemyStatus();
            AudioManager.Instance?.PlayBGM(BGMTrack.Battle_Normal);
        }

        private void OnBattleStateChanged(BattleState state)
        {
            bool showCommands = state == BattleState.PlayerTurn || state == BattleState.ResonanceChance;

            if (showCommands)
                UIAnimator.SlideInFromLeft(commandMenuGroup, commandMenuRect, 0.25f);
            else
                UIAnimator.FadeOut(commandMenuGroup, 0.15f);

            if (state == BattleState.ResonanceChance)
            {
                ShowMessage("RESONANCE! もう一度行動できる！");
                AudioManager.Instance?.PlaySFX(SFXType.Battle_Resonance);
            }

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
            {
                msg = $"{e.Attacker.Name}の攻撃は外れた！";
                AudioManager.Instance?.PlaySFX(SFXType.Battle_Miss);
                SpawnDamagePopup(0, e.Ability.element, false, false, false, true);
            }
            else if (e.Result.IsNulled)
            {
                msg = $"{e.Target.Name}には効かない！";
                AudioManager.Instance?.PlaySFX(SFXType.Battle_Null);
            }
            else if (e.Result.IsAbsorbed)
            {
                msg = $"{e.Target.Name}はダメージを吸収した！";
                AudioManager.Instance?.PlaySFX(SFXType.Battle_Absorb);
                SpawnDamagePopup(e.Result.Damage, e.Ability.element, false, false, true, false);
            }
            else if (e.Result.IsReflected)
            {
                msg = $"攻撃が反射された！";
                AudioManager.Instance?.PlaySFX(SFXType.Battle_Reflect);
            }
            else
            {
                msg = $"{e.Attacker.Name}の{e.Ability.abilityName}！ {e.Result.Damage}ダメージ！";

                if (e.Result.HitWeakness)
                {
                    msg += " 弱点！";
                    AudioManager.Instance?.PlaySFX(SFXType.Battle_WeakHit);
                    UIAnimator.ShakePosition(messageRect, 12f, 0.4f);
                }
                else if (e.Result.IsCritical)
                {
                    msg += " クリティカル！";
                    AudioManager.Instance?.PlaySFX(SFXType.Battle_CriticalHit);
                    UIAnimator.ShakePosition(messageRect, 8f, 0.3f);
                }
                else
                {
                    AudioManager.Instance?.PlaySFX(SFXType.Battle_Attack);
                }

                if (e.Result.IsResisted)
                    msg = $"{e.Attacker.Name}の{e.Ability.abilityName}！ {e.Result.Damage}ダメージ…（耐性）";

                bool isHeal = e.Ability.element == ElementType.Heal;
                SpawnDamagePopup(e.Result.Damage, e.Ability.element,
                    e.Result.HitWeakness, e.Result.IsCritical, isHeal, false);
            }

            ShowMessage(msg);
        }

        private void SpawnDamagePopup(int damage, ElementType element,
            bool isWeak, bool isCritical, bool isHealed, bool isMiss)
        {
            if (damagePopupPrefab == null || damagePopupContainer == null) return;
            var obj = Instantiate(damagePopupPrefab, damagePopupContainer);
            var popup = obj.GetComponent<DamagePopupUI>();
            popup?.Show(damage, element, isWeak, isCritical, isHealed, isMiss);
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
            UIAnimator.FadeOut(commandMenuGroup, 0.2f);
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

        private void ShowResonanceEffect(ElementType element)
        {
            if (resonanceOverlay == null) return;

            Color elementColor = UIColors.GetElementColor(element);
            if (resonanceWaveImage != null)
                resonanceWaveImage.color = elementColor;
            if (resonanceText != null)
                resonanceText.text = "RESONANCE";

            var rect = resonanceOverlay.GetComponent<RectTransform>();
            UIAnimator.PopIn(resonanceOverlay, rect, 0.3f);
            DOVirtual.DelayedCall(0.8f, () => UIAnimator.PopOut(resonanceOverlay, rect, 0.2f))
                .SetUpdate(true);
        }

        private void ShowFullResonanceAttackEffect(int totalDamage)
        {
            if (fullResonanceOverlay == null) return;

            AudioManager.Instance?.PlaySFX(SFXType.Battle_FullResonance);

            if (fullResonanceText != null)
                fullResonanceText.text = "FULL RESONANCE ATTACK";

            var rect = fullResonanceOverlay.GetComponent<RectTransform>();
            UIAnimator.PopIn(fullResonanceOverlay, rect, 0.4f);

            DOVirtual.DelayedCall(2f, () =>
            {
                if (fullResonanceText != null)
                    fullResonanceText.text = $"{totalDamage} DAMAGE!";
                UIAnimator.PunchScale(rect, 0.2f, 0.4f);
            }).SetUpdate(true);

            DOVirtual.DelayedCall(3.5f, () => UIAnimator.PopOut(fullResonanceOverlay, rect, 0.3f))
                .SetUpdate(true);
        }

        private void ShowMessage(string text)
        {
            if (battleMessageText == null) return;
            activeMsgTween?.Kill();
            battleMessageText.text = text;

            UIAnimator.SlideInFromBottom(messageGroup, messageRect, 0.2f);

            activeMsgTween = DOVirtual.DelayedCall(messageDisplayDuration, () =>
            {
                UIAnimator.FadeOut(messageGroup, 0.2f);
            }).SetUpdate(true);
        }

        private void ShowVictoryResult()
        {
            if (resultOverlay == null) return;
            if (resultTitleText != null) resultTitleText.text = "VICTORY";
            AudioManager.Instance?.PlayBGM(BGMTrack.Victory);
            UIAnimator.PopIn(resultOverlay, resultPanel, 0.5f);
        }

        private void ShowDefeatResult()
        {
            if (resultOverlay == null) return;
            if (resultTitleText != null) resultTitleText.text = "DEFEAT";
            AudioManager.Instance?.PlayBGM(BGMTrack.Defeat);
            UIAnimator.PopIn(resultOverlay, resultPanel, 0.5f);
        }

        private void OnAttackPressed()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            BattleFlowController.Instance?.OnCommandAttack();
        }

        private void OnAbilityMenuPressed()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            BattleFlowController.Instance?.OnCommandAbility();
        }

        private void OnGuardPressed()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            AudioManager.Instance?.PlaySFX(SFXType.Battle_Guard);
            BattleFlowController.Instance?.OnCommandGuard();
        }

        private void OnItemPressed()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Select);
        }

        private void OnEscapePressed()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            BattleFlowController.Instance?.OnCommandEscape();
        }
    }
}
