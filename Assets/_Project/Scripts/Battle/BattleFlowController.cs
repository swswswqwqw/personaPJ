using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Battle
{
    public enum BattleInputPhase
    {
        None,
        CommandSelect,
        TargetSelect,
        AbilitySelect,
        Executing,
        WaitingForAnimation
    }

    public class BattleFlowController : MonoBehaviour
    {
        public static BattleFlowController Instance { get; private set; }

        [Header("Target Selection")]
        [SerializeField] private GameObject targetCursorPrefab;
        [SerializeField] private RectTransform targetCursorContainer;
        [SerializeField] private TextMeshProUGUI targetNameText;
        [SerializeField] private CanvasGroup targetSelectGroup;

        [Header("Ability Selection")]
        [SerializeField] private CanvasGroup abilityListGroup;
        [SerializeField] private RectTransform abilityListContainer;
        [SerializeField] private GameObject abilityButtonPrefab;
        [SerializeField] private TextMeshProUGUI abilityDescText;
        [SerializeField] private TextMeshProUGUI abilityCostText;

        [Header("Execution Display")]
        [SerializeField] private CanvasGroup executionOverlay;
        [SerializeField] private TextMeshProUGUI executionText;

        [Header("Settings")]
        [SerializeField] private float executionDisplayTime = 1.2f;
        [SerializeField] private float turnTransitionDelay = 0.5f;

        public BattleInputPhase CurrentPhase { get; private set; }

        private BattleUnit currentUnit;
        private int selectedTargetIndex;
        private AbilityData selectedAbility;
        private bool isPlayerControlled;
        private readonly List<GameObject> activeAbilityButtons = new();

        public event Action<BattleInputPhase> OnInputPhaseChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
        }

        private void Update()
        {
            switch (CurrentPhase)
            {
                case BattleInputPhase.TargetSelect:
                    HandleTargetInput();
                    break;
            }
        }

        private void OnBattleStateChanged(BattleState state)
        {
            switch (state)
            {
                case BattleState.PlayerTurn:
                case BattleState.ResonanceChance:
                    BeginPlayerTurn();
                    break;
                case BattleState.EnemyTurn:
                    BeginEnemyTurn();
                    break;
                case BattleState.FullResonanceAttack:
                    HandleFullResonancePrompt();
                    break;
                case BattleState.Victory:
                case BattleState.Defeat:
                    SetPhase(BattleInputPhase.None);
                    break;
            }
        }

        private void BeginPlayerTurn()
        {
            currentUnit = GetCurrentTurnUnit();
            if (currentUnit == null) return;
            isPlayerControlled = true;
            SetPhase(BattleInputPhase.CommandSelect);
        }

        public void OnCommandAttack()
        {
            selectedAbility = GetBasicAttackAbility();
            SetPhase(BattleInputPhase.TargetSelect);
            ShowTargetSelection(getEnemies: true);
        }

        public void OnCommandAbility()
        {
            SetPhase(BattleInputPhase.AbilitySelect);
            ShowAbilityList();
        }

        public void OnCommandGuard()
        {
            SetPhase(BattleInputPhase.Executing);
            ShowExecution($"{currentUnit.Name}は身を守っている……");
            DelayedAdvanceTurn();
        }

        public void OnCommandEscape()
        {
            float escapeChance = 0.5f + currentUnit.Agility * 0.01f;
            bool escaped = UnityEngine.Random.value < escapeChance;

            if (escaped)
            {
                ShowExecution("逃走に成功した！");
                BattleManager.Instance?.EndBattle();
            }
            else
            {
                ShowExecution("逃げられなかった！");
                DelayedAdvanceTurn();
            }
        }

        public void OnAbilitySelected(AbilityData ability)
        {
            if (currentUnit.CurrentSP < ability.spCost)
            {
                ShowExecution("SPが足りない！");
                return;
            }

            selectedAbility = ability;
            HideAbilityList();

            bool targetEnemy = ability.targetType is TargetType.SingleEnemy or TargetType.AllEnemies;
            if (ability.targetType == TargetType.Self)
            {
                ExecuteAction(currentUnit);
                return;
            }

            if (ability.targetType is TargetType.AllEnemies or TargetType.AllAllies)
            {
                ExecuteActionAll(targetEnemy);
                return;
            }

            SetPhase(BattleInputPhase.TargetSelect);
            ShowTargetSelection(getEnemies: targetEnemy);
        }

        private void HandleTargetInput()
        {
            var targets = GetCurrentTargetList();
            if (targets == null || targets.Count == 0) return;

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                selectedTargetIndex = (selectedTargetIndex - 1 + targets.Count) % targets.Count;
                UpdateTargetCursor(targets);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                selectedTargetIndex = (selectedTargetIndex + 1) % targets.Count;
                UpdateTargetCursor(targets);
            }
            else if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return))
            {
                HideTargetSelection();
                ExecuteAction(targets[selectedTargetIndex]);
            }
            else if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.Escape))
            {
                HideTargetSelection();
                SetPhase(BattleInputPhase.CommandSelect);
            }
        }

        private async void ExecuteAction(BattleUnit target)
        {
            SetPhase(BattleInputPhase.Executing);

            if (selectedAbility == null) return;

            var result = BattleManager.Instance?.ExecuteAbility(currentUnit, target, selectedAbility);
            if (result == null) return;

            await Task.Delay((int)(executionDisplayTime * 1000));

            if (BattleManager.Instance?.CurrentState == BattleState.ResonanceChance)
            {
                SetPhase(BattleInputPhase.CommandSelect);
                return;
            }

            if (BattleManager.Instance?.CurrentState is BattleState.Victory or BattleState.Defeat
                or BattleState.FullResonanceAttack)
                return;

            await Task.Delay((int)(turnTransitionDelay * 1000));
            BattleManager.Instance?.AdvanceTurn();
        }

        private async void ExecuteActionAll(bool targetEnemies)
        {
            SetPhase(BattleInputPhase.Executing);
            if (selectedAbility == null) return;

            var targets = targetEnemies ? BattleManager.Instance?.EnemyUnits : BattleManager.Instance?.PartyUnits;
            if (targets == null) return;

            foreach (var target in targets)
            {
                if (!target.IsAlive) continue;
                BattleManager.Instance?.ExecuteAbility(currentUnit, target, selectedAbility);
                await Task.Delay(300);
            }

            await Task.Delay((int)(turnTransitionDelay * 1000));

            if (BattleManager.Instance?.CurrentState is BattleState.Victory or BattleState.Defeat
                or BattleState.FullResonanceAttack)
                return;

            BattleManager.Instance?.AdvanceTurn();
        }

        private async void HandleFullResonancePrompt()
        {
            SetPhase(BattleInputPhase.WaitingForAnimation);
            ShowExecution("全共鳴攻撃のチャンス！");

            await Task.Delay(1500);

            BattleManager.Instance?.ExecuteFullResonanceAttack();

            await Task.Delay(2500);

            if (BattleManager.Instance?.CurrentState is BattleState.Victory or BattleState.Defeat)
                return;

            BattleManager.Instance?.AdvanceTurn();
        }

        private async void BeginEnemyTurn()
        {
            currentUnit = GetCurrentTurnUnit();
            if (currentUnit == null) return;
            isPlayerControlled = false;
            SetPhase(BattleInputPhase.Executing);

            await Task.Delay(500);

            var action = EnemyAI.DecideAction(currentUnit, BattleManager.Instance);
            if (action.ability == null) return;

            ShowExecution($"{currentUnit.Name}の{action.ability.abilityName}！");
            BattleManager.Instance?.ExecuteAbility(currentUnit, action.target, action.ability);

            await Task.Delay((int)(executionDisplayTime * 1000));
            await Task.Delay((int)(turnTransitionDelay * 1000));

            if (BattleManager.Instance?.CurrentState is BattleState.Victory or BattleState.Defeat
                or BattleState.FullResonanceAttack)
                return;

            BattleManager.Instance?.AdvanceTurn();
        }

        private void ShowAbilityList()
        {
            ClearAbilityButtons();
            if (abilityListGroup == null || currentUnit == null) return;

            foreach (var ability in currentUnit.Abilities)
            {
                if (abilityButtonPrefab == null || abilityListContainer == null) continue;

                var btnObj = Instantiate(abilityButtonPrefab, abilityListContainer);
                activeAbilityButtons.Add(btnObj);

                var text = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    bool canUse = currentUnit.CurrentSP >= ability.spCost;
                    string color = canUse ? "white" : "#666666";
                    text.text = $"<color={color}>{ability.abilityName}  SP{ability.spCost}</color>";
                }

                var captured = ability;
                var btn = btnObj.GetComponent<Button>();
                btn?.onClick.AddListener(() => OnAbilitySelected(captured));

                if (btn != null)
                    btn.interactable = currentUnit.CurrentSP >= ability.spCost;
            }

            SetGroupVisible(abilityListGroup, true);
        }

        private void HideAbilityList()
        {
            ClearAbilityButtons();
            SetGroupVisible(abilityListGroup, false);
        }

        private void ShowTargetSelection(bool getEnemies)
        {
            selectedTargetIndex = 0;
            var targets = getEnemies ? BattleManager.Instance?.EnemyUnits : BattleManager.Instance?.PartyUnits;
            if (targets == null) return;

            while (selectedTargetIndex < targets.Count && !targets[selectedTargetIndex].IsAlive)
                selectedTargetIndex++;

            UpdateTargetCursor(targets);
            SetGroupVisible(targetSelectGroup, true);
        }

        private void HideTargetSelection()
        {
            SetGroupVisible(targetSelectGroup, false);
        }

        private void UpdateTargetCursor(List<BattleUnit> targets)
        {
            if (selectedTargetIndex >= targets.Count) return;
            var target = targets[selectedTargetIndex];
            if (targetNameText != null)
                targetNameText.text = target.Name;
        }

        private async void ShowExecution(string text)
        {
            if (executionText != null) executionText.text = text;
            SetGroupVisible(executionOverlay, true);
            await Task.Delay((int)(executionDisplayTime * 1000));
            SetGroupVisible(executionOverlay, false);
        }

        private BattleUnit GetCurrentTurnUnit()
        {
            var bm = BattleManager.Instance;
            if (bm == null || bm.TurnOrder.Count == 0) return null;
            return bm.TurnOrder[bm.CurrentTurnIndex];
        }

        private List<BattleUnit> GetCurrentTargetList()
        {
            if (selectedAbility == null) return BattleManager.Instance?.EnemyUnits;

            return selectedAbility.targetType switch
            {
                TargetType.SingleAlly => BattleManager.Instance?.PartyUnits,
                _ => BattleManager.Instance?.EnemyUnits
            };
        }

        private AbilityData GetBasicAttackAbility()
        {
            var attack = ScriptableObject.CreateInstance<AbilityData>();
            attack.abilityName = "通常攻撃";
            attack.element = ElementType.Strike;
            attack.targetType = TargetType.SingleEnemy;
            attack.basePower = 30;
            attack.spCost = 0;
            attack.accuracy = 0.95f;
            attack.criticalRate = 10;
            return attack;
        }

        private void SetPhase(BattleInputPhase phase)
        {
            CurrentPhase = phase;
            OnInputPhaseChanged?.Invoke(phase);
        }

        private async void DelayedAdvanceTurn()
        {
            await Task.Delay((int)(executionDisplayTime * 1000));
            await Task.Delay((int)(turnTransitionDelay * 1000));
            BattleManager.Instance?.AdvanceTurn();
        }

        private void ClearAbilityButtons()
        {
            foreach (var btn in activeAbilityButtons)
                if (btn != null) Destroy(btn);
            activeAbilityButtons.Clear();
        }

        private void SetGroupVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
