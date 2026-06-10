using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArcadiaOfEchoes.Dialogue;

namespace ArcadiaOfEchoes.UI
{
    public class DialogueUI : UIPanel
    {
        [Header("Dialogue Box")]
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Choices")]
        [SerializeField] private GameObject choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Character Display")]
        [SerializeField] private Image speakerPortrait;

        [Header("Settings")]
        [SerializeField] private float textSpeed = 0.03f;

        private readonly List<GameObject> _activeChoiceButtons = new();
        private bool _isTyping;
        private string _fullText;
        private int _currentCharIndex;
        private float _nextCharTime;

        private void OnEnable()
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.OnDialogueNodeStarted += ShowNode;
                DialogueSystem.Instance.OnDialogueEnded += HideDialogue;
            }
        }

        private void OnDisable()
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.OnDialogueNodeStarted -= ShowNode;
                DialogueSystem.Instance.OnDialogueEnded -= HideDialogue;
            }
        }

        private void Update()
        {
            if (!_isTyping) return;

            if (Time.time >= _nextCharTime && _currentCharIndex < _fullText.Length)
            {
                _currentCharIndex++;
                dialogueText.text = _fullText[.._currentCharIndex];
                _nextCharTime = Time.time + textSpeed;

                if (_currentCharIndex >= _fullText.Length)
                {
                    _isTyping = false;
                    ShowChoicesIfAny();
                }
            }
        }

        public void OnClick()
        {
            if (_isTyping)
            {
                _currentCharIndex = _fullText.Length;
                dialogueText.text = _fullText;
                _isTyping = false;
                ShowChoicesIfAny();
            }
            else if (DialogueSystem.Instance?.CurrentNode?.HasChoices != true)
            {
                DialogueSystem.Instance?.AdvanceDialogue();
            }
        }

        private void ShowNode(DialogueNode node)
        {
            dialogueBox?.SetActive(true);
            choiceContainer?.SetActive(false);
            ClearChoiceButtons();

            if (speakerNameText != null)
                speakerNameText.text = node.SpeakerName ?? "";

            _fullText = node.Text ?? "";
            _currentCharIndex = 0;
            _isTyping = true;
            _nextCharTime = Time.time;

            if (dialogueText != null)
                dialogueText.text = "";
        }

        private void ShowChoicesIfAny()
        {
            var node = DialogueSystem.Instance?.CurrentNode;
            if (node == null || !node.HasChoices) return;

            choiceContainer?.SetActive(true);

            for (int i = 0; i < node.Choices.Count; i++)
            {
                if (choiceButtonPrefab == null || choiceContainer == null) break;

                var buttonObj = Instantiate(choiceButtonPrefab, choiceContainer.transform);
                var tmpText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.text = node.Choices[i].ChoiceText;

                int choiceIndex = i;
                var button = buttonObj.GetComponent<Button>();
                button?.onClick.AddListener(() => OnChoiceSelected(choiceIndex));

                _activeChoiceButtons.Add(buttonObj);
            }
        }

        private void OnChoiceSelected(int index)
        {
            ClearChoiceButtons();
            choiceContainer?.SetActive(false);
            DialogueSystem.Instance?.SelectChoice(index);
        }

        private void ClearChoiceButtons()
        {
            foreach (var btn in _activeChoiceButtons)
                Destroy(btn);
            _activeChoiceButtons.Clear();
        }

        private void HideDialogue()
        {
            dialogueBox?.SetActive(false);
            choiceContainer?.SetActive(false);
            ClearChoiceButtons();
        }
    }
}
