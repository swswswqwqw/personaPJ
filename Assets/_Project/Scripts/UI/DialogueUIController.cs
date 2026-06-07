using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Dialogue;

namespace EchoesOfArcadia.UI
{
    public class DialogueUIController : MonoBehaviour
    {
        [Header("Dialogue Window")]
        [SerializeField] private CanvasGroup dialogueWindowGroup;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image speakerNameBackground;
        [SerializeField] private Image windowBackground;
        [SerializeField] private GameObject advanceIndicator;

        [Header("Choice Panel")]
        [SerializeField] private CanvasGroup choiceGroup;
        [SerializeField] private RectTransform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Character Portrait")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private CanvasGroup portraitGroup;

        [Header("Typewriter Settings")]
        [SerializeField] private float typewriterSpeed = 0.03f;
        [SerializeField] private float punctuationPause = 0.15f;

        [Header("Wave Decoration")]
        [SerializeField] private RectTransform[] waveDecorations;
        [SerializeField] private float waveSpeed = 2f;
        [SerializeField] private float waveAmplitude = 3f;

        private bool isTyping;
        private bool skipRequested;
        private CancellationTokenSource typewriterCts;
        private readonly List<GameObject> activeChoiceButtons = new();

        private void OnEnable()
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.OnLineDisplayed += OnLineDisplayed;
                DialogueSystem.Instance.OnChoicesPresented += OnChoicesPresented;
                DialogueSystem.Instance.OnDialogueEnded += OnDialogueEnded;
            }

            SetOverlayVisible(dialogueWindowGroup, false);
            SetOverlayVisible(choiceGroup, false);
        }

        private void OnDisable()
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.OnLineDisplayed -= OnLineDisplayed;
                DialogueSystem.Instance.OnChoicesPresented -= OnChoicesPresented;
                DialogueSystem.Instance.OnDialogueEnded -= OnDialogueEnded;
            }

            typewriterCts?.Cancel();
        }

        private void Update()
        {
            if (!DialogueSystem.Instance?.IsActive ?? true) return;

            AnimateWaveDecorations();

            if (Input.GetButtonDown("Submit") || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    skipRequested = true;
                }
                else if (choiceGroup == null || choiceGroup.alpha < 0.5f)
                {
                    DialogueSystem.Instance.AdvanceLine();
                }
            }
        }

        private void OnLineDisplayed(DialogueLine line)
        {
            SetOverlayVisible(dialogueWindowGroup, true);
            SetOverlayVisible(choiceGroup, false);

            if (speakerNameText != null)
            {
                speakerNameText.text = line.speakerName ?? "";
                bool hasSpeaker = !string.IsNullOrEmpty(line.speakerName);
                if (speakerNameBackground != null)
                    speakerNameBackground.gameObject.SetActive(hasSpeaker);
            }

            if (advanceIndicator != null)
                advanceIndicator.SetActive(false);

            typewriterCts?.Cancel();
            typewriterCts = new CancellationTokenSource();
            TypeText(line.text, typewriterCts.Token);
        }

        private void OnChoicesPresented(List<DialogueChoice> choices)
        {
            ClearChoiceButtons();
            SetOverlayVisible(choiceGroup, true);

            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                int index = i;

                if (choiceButtonPrefab == null || choiceContainer == null) continue;

                var buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
                activeChoiceButtons.Add(buttonObj);

                var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string displayText = choice.text;

                    if (choice.statRequirement.HasValue)
                    {
                        bool meetsRequirement = Data.PlayerStats.Instance?.MeetsRequirement(
                            choice.statRequirement.Value, choice.requiredRank) ?? false;

                        if (!meetsRequirement)
                            displayText = $"<color=#666666>[{choice.statRequirement.Value} 不足] {choice.text}</color>";
                    }

                    buttonText.text = displayText;
                }

                var button = buttonObj.GetComponent<Button>();
                button?.onClick.AddListener(() =>
                {
                    SetOverlayVisible(choiceGroup, false);
                    ClearChoiceButtons();
                    DialogueSystem.Instance?.SelectChoice(index);
                });

                if (choice.statRequirement.HasValue)
                {
                    bool meetsReq = Data.PlayerStats.Instance?.MeetsRequirement(
                        choice.statRequirement.Value, choice.requiredRank) ?? false;
                    if (button != null) button.interactable = meetsReq;
                }
            }
        }

        private void OnDialogueEnded()
        {
            SetOverlayVisible(dialogueWindowGroup, false);
            SetOverlayVisible(choiceGroup, false);
            ClearChoiceButtons();
        }

        private async void TypeText(string fullText, CancellationToken token)
        {
            isTyping = true;
            skipRequested = false;
            if (dialogueText != null) dialogueText.text = "";

            for (int i = 0; i < fullText.Length; i++)
            {
                if (token.IsCancellationRequested) return;

                if (skipRequested)
                {
                    if (dialogueText != null) dialogueText.text = fullText;
                    break;
                }

                if (dialogueText != null)
                    dialogueText.text = fullText[..(i + 1)];

                char c = fullText[i];
                float delay = IsPunctuation(c) ? punctuationPause : typewriterSpeed;
                await Task.Delay((int)(delay * 1000), token);
            }

            isTyping = false;
            if (advanceIndicator != null)
                advanceIndicator.SetActive(true);
        }

        private void ClearChoiceButtons()
        {
            foreach (var btn in activeChoiceButtons)
            {
                if (btn != null) Destroy(btn);
            }
            activeChoiceButtons.Clear();
        }

        private void AnimateWaveDecorations()
        {
            if (waveDecorations == null) return;
            float t = Time.time;

            for (int i = 0; i < waveDecorations.Length; i++)
            {
                if (waveDecorations[i] == null) continue;
                float offset = i * 0.3f;
                float y = Mathf.Sin((t + offset) * waveSpeed) * waveAmplitude;
                var pos = waveDecorations[i].anchoredPosition;
                pos.y = y;
                waveDecorations[i].anchoredPosition = pos;
            }
        }

        private static bool IsPunctuation(char c) =>
            c is '。' or '、' or '！' or '？' or '…' or '—' or '.' or ',' or '!' or '?';

        private void SetOverlayVisible(CanvasGroup group, bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
