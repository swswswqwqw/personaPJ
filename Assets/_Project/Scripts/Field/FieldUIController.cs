using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Field
{
    public class FieldUIController : MonoBehaviour
    {
        [Header("Location Info")]
        [SerializeField] private TextMeshProUGUI locationNameText;
        [SerializeField] private TextMeshProUGUI locationDescText;

        [Header("Navigation Panel")]
        [SerializeField] private CanvasGroup navigationGroup;
        [SerializeField] private RectTransform navigationContainer;
        [SerializeField] private GameObject navigationButtonPrefab;

        [Header("NPC Panel")]
        [SerializeField] private CanvasGroup npcGroup;
        [SerializeField] private RectTransform npcContainer;
        [SerializeField] private GameObject npcButtonPrefab;

        [Header("Activity Panel")]
        [SerializeField] private CanvasGroup activityGroup;
        [SerializeField] private RectTransform activityContainer;
        [SerializeField] private GameObject activityButtonPrefab;

        [Header("Interaction Buttons")]
        [SerializeField] private Button moveButton;
        [SerializeField] private Button talkButton;
        [SerializeField] private Button actButton;

        private readonly List<GameObject> dynamicButtons = new();

        private void OnEnable()
        {
            if (FieldManager.Instance != null)
            {
                FieldManager.Instance.OnLocationChanged += OnLocationChanged;
                FieldManager.Instance.OnNPCsRefreshed += OnNPCsRefreshed;
            }

            moveButton?.onClick.AddListener(ShowNavigationPanel);
            talkButton?.onClick.AddListener(ShowNPCPanel);
            actButton?.onClick.AddListener(ShowActivityPanel);
        }

        private void OnDisable()
        {
            if (FieldManager.Instance != null)
            {
                FieldManager.Instance.OnLocationChanged -= OnLocationChanged;
                FieldManager.Instance.OnNPCsRefreshed -= OnNPCsRefreshed;
            }

            moveButton?.onClick.RemoveAllListeners();
            talkButton?.onClick.RemoveAllListeners();
            actButton?.onClick.RemoveAllListeners();
        }

        private void OnLocationChanged(LocationData location)
        {
            if (locationNameText != null) locationNameText.text = location.locationName;
            if (locationDescText != null) locationDescText.text = location.description;
            HideAllPanels();

            bool hasNPCs = FieldManager.Instance?.AvailableNPCs.Count > 0;
            if (talkButton != null) talkButton.interactable = hasNPCs;

            bool hasActivities = FieldManager.Instance?.GetAvailableActivities().Count > 0;
            if (actButton != null) actButton.interactable = hasActivities;
        }

        private void OnNPCsRefreshed(List<NPCPlacement> npcs)
        {
            if (talkButton != null) talkButton.interactable = npcs.Count > 0;
        }

        private void ShowNavigationPanel()
        {
            HideAllPanels();
            ClearDynamicButtons();

            var connected = FieldManager.Instance?.GetConnectedLocations();
            if (connected == null) return;

            foreach (var loc in connected)
            {
                CreateButton(navigationContainer, loc.locationName, () =>
                {
                    FieldManager.Instance?.MoveToLocation(loc);
                    HideAllPanels();
                });
            }

            SetGroupVisible(navigationGroup, true);
        }

        private void ShowNPCPanel()
        {
            HideAllPanels();
            ClearDynamicButtons();

            var npcs = FieldManager.Instance?.AvailableNPCs;
            if (npcs == null) return;

            foreach (var npc in npcs)
            {
                string label = npc.character != null ? npc.character.characterName : "???";
                if (npc.isBondTarget)
                    label += " <size=70%><color=#00D4AA>[絆の調べ]</color></size>";

                CreateButton(npcContainer, label, () =>
                {
                    FieldManager.Instance?.InteractWithNPC(npc);
                    HideAllPanels();
                });
            }

            SetGroupVisible(npcGroup, true);
        }

        private void ShowActivityPanel()
        {
            HideAllPanels();
            ClearDynamicButtons();

            var activities = FieldManager.Instance?.GetAvailableActivities();
            if (activities == null) return;

            foreach (var activity in activities)
            {
                string label = $"{activity.activityName}\n<size=70%>{activity.description} (+{activity.statPoints} {GetStatName(activity.statToRaise)})</size>";

                CreateButton(activityContainer, label, () =>
                {
                    FieldManager.Instance?.PerformActivity(activity);
                    HideAllPanels();
                });
            }

            SetGroupVisible(activityGroup, true);
        }

        private void CreateButton(RectTransform container, string text, System.Action onClick)
        {
            if (navigationButtonPrefab == null || container == null) return;

            var btnObj = Instantiate(navigationButtonPrefab, container);
            dynamicButtons.Add(btnObj);

            var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = text;

            var btn = btnObj.GetComponent<Button>();
            btn?.onClick.AddListener(() => onClick());
        }

        private void HideAllPanels()
        {
            SetGroupVisible(navigationGroup, false);
            SetGroupVisible(npcGroup, false);
            SetGroupVisible(activityGroup, false);
            ClearDynamicButtons();
        }

        private void ClearDynamicButtons()
        {
            foreach (var btn in dynamicButtons)
                if (btn != null) Destroy(btn);
            dynamicButtons.Clear();
        }

        private static string GetStatName(PersonalStat stat) => stat switch
        {
            PersonalStat.Insight => "洞察",
            PersonalStat.Courage => "勇気",
            PersonalStat.Empathy => "共感",
            PersonalStat.Expression => "表現",
            PersonalStat.Endurance => "忍耐",
            _ => ""
        };

        private void SetGroupVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
