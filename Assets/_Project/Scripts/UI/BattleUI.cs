using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArcadiaOfEchoes.UI
{
    public class BattleUI : UIPanel
    {
        [Header("Command Menu")]
        [SerializeField] private GameObject commandPanel;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button skillButton;
        [SerializeField] private Button resonanceFormButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button escapeButton;

        [Header("Party Status")]
        [SerializeField] private List<PartyMemberHUD> partyHUDs;

        [Header("Enemy Display")]
        [SerializeField] private Transform enemyHUDContainer;

        [Header("Turn Order")]
        [SerializeField] private Transform turnOrderContainer;

        [Header("Result")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("Echo Strike (1-More)")]
        [SerializeField] private GameObject echoStrikeEffect;
        [SerializeField] private TextMeshProUGUI echoStrikeText;

        [Header("Harmony Break")]
        [SerializeField] private GameObject harmonyBreakPanel;
        [SerializeField] private Button harmonyBreakButton;

        public event System.Action<BattleCommand> OnCommandSelected;

        public override void OnActivate()
        {
            commandPanel?.SetActive(false);
            victoryPanel?.SetActive(false);
            defeatPanel?.SetActive(false);
            harmonyBreakPanel?.SetActive(false);
        }

        public void ShowCommandMenu()
        {
            commandPanel?.SetActive(true);
        }

        public void HideCommandMenu()
        {
            commandPanel?.SetActive(false);
        }

        public void ShowEchoStrike()
        {
            if (echoStrikeEffect == null) return;
            echoStrikeEffect.SetActive(true);
            if (echoStrikeText != null)
                echoStrikeText.text = "残響撃！";
            // DOTween: echoStrikeEffect.transform.DOScale(1.2f, 0.3f).SetLoops(2, LoopType.Yoyo);
        }

        public void ShowHarmonyBreakPrompt()
        {
            harmonyBreakPanel?.SetActive(true);
        }

        public void ShowVictory(int expGained, int moneyGained)
        {
            victoryPanel?.SetActive(true);
            commandPanel?.SetActive(false);
        }

        public void ShowDefeat()
        {
            defeatPanel?.SetActive(true);
            commandPanel?.SetActive(false);
        }

        public void UpdatePartyHUD(int index, string name, int currentHP, int maxHP, int currentSP, int maxSP)
        {
            if (index < 0 || index >= partyHUDs.Count) return;
            partyHUDs[index].UpdateDisplay(name, currentHP, maxHP, currentSP, maxSP);
        }

        private void SetupButtonListeners()
        {
            attackButton?.onClick.AddListener(() => OnCommandSelected?.Invoke(BattleCommand.Attack));
            skillButton?.onClick.AddListener(() => OnCommandSelected?.Invoke(BattleCommand.Skill));
            resonanceFormButton?.onClick.AddListener(() => OnCommandSelected?.Invoke(BattleCommand.ResonanceForm));
            itemButton?.onClick.AddListener(() => OnCommandSelected?.Invoke(BattleCommand.Item));
            escapeButton?.onClick.AddListener(() => OnCommandSelected?.Invoke(BattleCommand.Escape));
        }

        private void OnEnable()
        {
            SetupButtonListeners();
        }
    }

    public enum BattleCommand
    {
        Attack,
        Skill,
        ResonanceForm,
        Item,
        Escape,
        HarmonyBreak
    }

    [System.Serializable]
    public class PartyMemberHUD
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI HPText;
        public TextMeshProUGUI SPText;
        public Slider HPBar;
        public Slider SPBar;

        public void UpdateDisplay(string name, int currentHP, int maxHP, int currentSP, int maxSP)
        {
            if (NameText != null) NameText.text = name;
            if (HPText != null) HPText.text = $"{currentHP}/{maxHP}";
            if (SPText != null) SPText.text = $"{currentSP}/{maxSP}";
            if (HPBar != null) HPBar.value = maxHP > 0 ? (float)currentHP / maxHP : 0;
            if (SPBar != null) SPBar.value = maxSP > 0 ? (float)currentSP / maxSP : 0;
        }
    }
}
