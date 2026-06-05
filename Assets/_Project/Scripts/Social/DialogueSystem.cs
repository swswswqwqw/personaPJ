using System;
using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Core;

namespace ArcadiaOfEchoes.Social
{
    [Serializable]
    public class DialogueData
    {
        public string DialogueId;
        public DialogueLine[] Lines;
    }

    [Serializable]
    public class DialogueLine
    {
        public string SpeakerId;
        public string Text;
        public string Emotion;
        public DialogueChoice[] Choices;
        public string NextLineId;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string Text;
        public string NextLineId;
        public int BondPoints;
        public InnerFrequency? StatBoost;
        public int StatAmount;
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        private DialogueData _currentDialogue;
        private int _currentLineIndex;
        private bool _isActive;
        private bool _waitingForChoice;

        public bool IsActive => _isActive;
        public DialogueLine CurrentLine => _isActive && _currentLineIndex < _currentDialogue.Lines.Length
            ? _currentDialogue.Lines[_currentLineIndex]
            : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartDialogue(DialogueData dialogue)
        {
            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            _isActive = true;
            _waitingForChoice = false;

            EventBus.Publish(new DialogueStartedEvent(dialogue.DialogueId));
            ShowCurrentLine();
        }

        public void AdvanceLine()
        {
            if (!_isActive || _waitingForChoice) return;

            _currentLineIndex++;
            if (_currentLineIndex >= _currentDialogue.Lines.Length)
            {
                EndDialogue();
                return;
            }

            ShowCurrentLine();
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!_waitingForChoice) return;

            var line = CurrentLine;
            if (line?.Choices == null || choiceIndex >= line.Choices.Length) return;

            var choice = line.Choices[choiceIndex];
            _waitingForChoice = false;

            if (choice.BondPoints > 0 && !string.IsNullOrEmpty(line.SpeakerId))
                ResonanceBondManager.Instance?.AddBondPoints(line.SpeakerId, choice.BondPoints);

            if (choice.StatBoost.HasValue && choice.StatAmount > 0)
                PlayerStats.Instance?.AddStat(choice.StatBoost.Value, choice.StatAmount);

            EventBus.Publish(new DialogueChoiceMadeEvent(choiceIndex, choice.Text));
            AdvanceLine();
        }

        private void ShowCurrentLine()
        {
            var line = CurrentLine;
            if (line == null) return;

            bool hasChoices = line.Choices != null && line.Choices.Length > 0;
            _waitingForChoice = hasChoices;

            EventBus.Publish(new DialogueLineShownEvent(line, hasChoices));
        }

        private void EndDialogue()
        {
            string dialogueId = _currentDialogue.DialogueId;
            _isActive = false;
            _currentDialogue = null;
            _currentLineIndex = 0;

            EventBus.Publish(new DialogueEndedEvent(dialogueId));
        }
    }

    public readonly struct DialogueStartedEvent
    {
        public readonly string DialogueId;
        public DialogueStartedEvent(string id) { DialogueId = id; }
    }

    public readonly struct DialogueLineShownEvent
    {
        public readonly DialogueLine Line;
        public readonly bool HasChoices;
        public DialogueLineShownEvent(DialogueLine line, bool hasChoices) { Line = line; HasChoices = hasChoices; }
    }

    public readonly struct DialogueChoiceMadeEvent
    {
        public readonly int ChoiceIndex;
        public readonly string ChoiceText;
        public DialogueChoiceMadeEvent(int index, string text) { ChoiceIndex = index; ChoiceText = text; }
    }

    public readonly struct DialogueEndedEvent
    {
        public readonly string DialogueId;
        public DialogueEndedEvent(string id) { DialogueId = id; }
    }
}
