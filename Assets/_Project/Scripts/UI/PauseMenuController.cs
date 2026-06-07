using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Menu Panel")]
        [SerializeField] private CanvasGroup menuGroup;
        [SerializeField] private Button statusButton;
        [SerializeField] private Button bondButton;
        [SerializeField] private Button calendarButton;
        [SerializeField] private Button equipmentButton;
        [SerializeField] private Button systemButton;
        [SerializeField] private Button closeButton;

        [Header("Status Panel")]
        [SerializeField] private CanvasGroup statusGroup;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private PersonalStatDisplay[] statDisplays;

        [Header("System Panel")]
        [SerializeField] private CanvasGroup systemGroup;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button titleButton;

        [Header("Sub-Controllers")]
        [SerializeField] private BondUIController bondUI;
        [SerializeField] private CalendarUIController calendarUI;

        private bool isOpen;

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (isOpen) Close();
                else Open();
            }
        }

        public void Open()
        {
            if (GameManager.Instance?.CurrentPhase == GamePhase.Battle) return;
            if (GameManager.Instance?.CurrentPhase == GamePhase.Dialogue) return;

            isOpen = true;
            GameManager.Instance?.Pause();
            SetVisible(menuGroup, true);
            HideAllSubPanels();

            statusButton?.onClick.AddListener(ShowStatus);
            bondButton?.onClick.AddListener(ShowBonds);
            calendarButton?.onClick.AddListener(ShowCalendar);
            systemButton?.onClick.AddListener(ShowSystem);
            closeButton?.onClick.AddListener(Close);
        }

        public void Close()
        {
            isOpen = false;
            GameManager.Instance?.Resume();
            SetVisible(menuGroup, false);
            HideAllSubPanels();

            statusButton?.onClick.RemoveAllListeners();
            bondButton?.onClick.RemoveAllListeners();
            calendarButton?.onClick.RemoveAllListeners();
            systemButton?.onClick.RemoveAllListeners();
            closeButton?.onClick.RemoveAllListeners();
        }

        private void ShowStatus()
        {
            HideAllSubPanels();
            SetVisible(statusGroup, true);
            RefreshStats();
        }

        private void ShowBonds()
        {
            HideAllSubPanels();
            bondUI?.OpenBondMap();
        }

        private void ShowCalendar()
        {
            HideAllSubPanels();
            calendarUI?.Open();
        }

        private void ShowSystem()
        {
            HideAllSubPanels();
            SetVisible(systemGroup, true);

            saveButton?.onClick.RemoveAllListeners();
            loadButton?.onClick.RemoveAllListeners();
            titleButton?.onClick.RemoveAllListeners();

            saveButton?.onClick.AddListener(() => SaveManager.Instance?.Save(0));
            loadButton?.onClick.AddListener(() =>
            {
                var data = SaveManager.Instance?.Load(0);
                if (data != null)
                    GameEventBus.Publish(new GameLoadedEvent(data));
            });
            titleButton?.onClick.AddListener(() =>
            {
                Close();
                SceneLoader.Instance?.LoadScene("Title", GamePhase.Title);
            });
        }

        private void RefreshStats()
        {
            if (PlayerStats.Instance == null || statDisplays == null) return;

            foreach (var display in statDisplays)
            {
                if (display == null) continue;
                display.Refresh();
            }
        }

        private void HideAllSubPanels()
        {
            SetVisible(statusGroup, false);
            SetVisible(systemGroup, false);
            bondUI?.CloseBondMap();
            calendarUI?.Close();
        }

        private void SetVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }

    [System.Serializable]
    public class PersonalStatDisplay : MonoBehaviour
    {
        [SerializeField] private PersonalStat stat;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI rankNameText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image fillImage;

        public void Refresh()
        {
            if (PlayerStats.Instance == null) return;

            string statName = stat switch
            {
                PersonalStat.Insight => "洞察",
                PersonalStat.Courage => "勇気",
                PersonalStat.Empathy => "共感",
                PersonalStat.Expression => "表現",
                PersonalStat.Endurance => "忍耐",
                _ => ""
            };

            if (nameText != null) nameText.text = statName;
            if (rankNameText != null) rankNameText.text = PlayerStats.Instance.GetRankName(stat);

            if (progressBar != null)
            {
                int points = PlayerStats.Instance.GetPoints(stat);
                progressBar.maxValue = 140;
                progressBar.value = Mathf.Min(points, 140);
            }

            if (fillImage != null) fillImage.color = UIColors.Cyan;
        }
    }
}
