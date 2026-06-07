using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.TimeSystem;

namespace EchoesOfArcadia.UI
{
    [Serializable]
    public class ActionOption
    {
        public string actionName;
        public string locationName;
        public string description;
        public int actionPointCost = 1;
        public Sprite locationIcon;
        public Action onSelected;
    }

    public class ActionSelectUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private RectTransform optionContainer;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("Detail Panel")]
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI actionPointCostText;

        [Header("Time Indicator")]
        [SerializeField] private Image clockImage;
        [SerializeField] private TextMeshProUGUI timeRemainingText;

        private readonly List<GameObject> activeButtons = new();

        public void Show(List<ActionOption> options)
        {
            ClearButtons();

            if (TimeManager.Instance != null)
            {
                string timeName = TimeManager.Instance.CurrentTimeOfDay switch
                {
                    TimeOfDay.Afternoon => "放課後",
                    TimeOfDay.Evening => "夜",
                    TimeOfDay.LateNight => "深夜",
                    _ => ""
                };
                if (headerText != null) headerText.text = $"{timeName}——何をする？";
                if (timeRemainingText != null)
                    timeRemainingText.text = $"残り行動力: {TimeManager.Instance.RemainingActionPoints}";
            }

            foreach (var option in options)
            {
                CreateOptionButton(option);
            }

            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
            ClearButtons();
        }

        private void CreateOptionButton(ActionOption option)
        {
            if (optionButtonPrefab == null || optionContainer == null) return;

            var buttonObj = Instantiate(optionButtonPrefab, optionContainer);
            activeButtons.Add(buttonObj);

            var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = $"{option.locationName}\n<size=70%>{option.actionName}</size>";

            var button = buttonObj.GetComponent<Button>();
            button?.onClick.AddListener(() =>
            {
                ShowDetail(option);
            });

            var eventTrigger = buttonObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                // ホバー時にdetailを表示するためのEventTrigger設定は
                // Prefab側で行う（Inspector設定）
            }
        }

        private void ShowDetail(ActionOption option)
        {
            if (detailNameText != null) detailNameText.text = option.actionName;
            if (detailDescriptionText != null) detailDescriptionText.text = option.description;
            if (actionPointCostText != null) actionPointCostText.text = $"行動力消費: {option.actionPointCost}";
        }

        public void ConfirmSelection(ActionOption option)
        {
            if (TimeManager.Instance != null && !TimeManager.Instance.SpendActionPoint())
                return;

            option.onSelected?.Invoke();
            Hide();
        }

        private void ClearButtons()
        {
            foreach (var btn in activeButtons)
            {
                if (btn != null) Destroy(btn);
            }
            activeButtons.Clear();
        }

        private void SetVisible(bool visible)
        {
            if (panelGroup == null) return;
            panelGroup.alpha = visible ? 1f : 0f;
            panelGroup.interactable = visible;
            panelGroup.blocksRaycasts = visible;
        }
    }
}
