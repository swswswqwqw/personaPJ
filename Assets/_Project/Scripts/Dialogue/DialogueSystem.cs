using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;

namespace EchoesOfArcadia.Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public string expression;
        public float delay;
        public bool isEcho;
        public List<DialogueChoice> choices;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public string nextNodeId;
        public int resonancePoints;
        public string requiredStat;
        public int requiredStatLevel;
    }

    [Serializable]
    public class DialogueNode
    {
        public string nodeId;
        public List<DialogueLine> lines;
        public string nextNodeId;
    }

    [Serializable]
    public class DialogueScript
    {
        public string scriptId;
        public string title;
        public List<DialogueNode> nodes;
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        private DialogueScript _currentScript;
        private int _currentNodeIndex;
        private int _currentLineIndex;
        private bool _isActive;
        private bool _waitingForChoice;

        public bool IsActive => _isActive;
        public bool WaitingForChoice => _waitingForChoice;
        public DialogueLine CurrentLine { get; private set; }

        public event Action<DialogueLine> OnLineDisplayed;
        public event Action<List<DialogueChoice>> OnChoicesPresented;
        public event Action<DialogueChoice> OnChoiceSelected;
        public event Action OnDialogueStarted;
        public event Action OnDialogueEnded;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartDialogue(DialogueScript script)
        {
            _currentScript = script;
            _currentNodeIndex = 0;
            _currentLineIndex = 0;
            _isActive = true;

            OnDialogueStarted?.Invoke();
            GameEventBus.Publish(new DialogueStartedEvent(script.scriptId));

            DisplayCurrentLine();
        }

        public void AdvanceLine()
        {
            if (!_isActive || _waitingForChoice) return;

            _currentLineIndex++;

            var node = _currentScript.nodes[_currentNodeIndex];
            if (_currentLineIndex >= node.lines.Count)
            {
                AdvanceNode(node.nextNodeId);
                return;
            }

            DisplayCurrentLine();
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!_waitingForChoice) return;

            var choices = CurrentLine.choices;
            if (choiceIndex < 0 || choiceIndex >= choices.Count) return;

            var choice = choices[choiceIndex];
            _waitingForChoice = false;

            OnChoiceSelected?.Invoke(choice);
            GameEventBus.Publish(new DialogueChoiceEvent(choice));

            if (choice.resonancePoints > 0)
                GameEventBus.Publish(new DialogueResonanceEvent(choice.resonancePoints));

            AdvanceNode(choice.nextNodeId);
        }

        private void AdvanceNode(string nextNodeId)
        {
            if (string.IsNullOrEmpty(nextNodeId))
            {
                EndDialogue();
                return;
            }

            for (int i = 0; i < _currentScript.nodes.Count; i++)
            {
                if (_currentScript.nodes[i].nodeId == nextNodeId)
                {
                    _currentNodeIndex = i;
                    _currentLineIndex = 0;
                    DisplayCurrentLine();
                    return;
                }
            }

            EndDialogue();
        }

        private void DisplayCurrentLine()
        {
            var node = _currentScript.nodes[_currentNodeIndex];
            CurrentLine = node.lines[_currentLineIndex];

            OnLineDisplayed?.Invoke(CurrentLine);
            GameEventBus.Publish(new DialogueLineEvent(CurrentLine));

            if (CurrentLine.choices != null && CurrentLine.choices.Count > 0)
            {
                _waitingForChoice = true;
                OnChoicesPresented?.Invoke(CurrentLine.choices);
            }
        }

        private void EndDialogue()
        {
            _isActive = false;
            _currentScript = null;
            CurrentLine = null;

            OnDialogueEnded?.Invoke();
            GameEventBus.Publish(new DialogueEndedEvent());
        }

        public DialogueScript LoadFromJson(string json)
        {
            return JsonUtility.FromJson<DialogueScript>(json);
        }
    }

    public readonly struct DialogueStartedEvent
    {
        public readonly string ScriptId;
        public DialogueStartedEvent(string id) => ScriptId = id;
    }

    public readonly struct DialogueEndedEvent { }

    public readonly struct DialogueLineEvent
    {
        public readonly DialogueLine Line;
        public DialogueLineEvent(DialogueLine line) => Line = line;
    }

    public readonly struct DialogueChoiceEvent
    {
        public readonly DialogueChoice Choice;
        public DialogueChoiceEvent(DialogueChoice choice) => Choice = choice;
    }

    public readonly struct DialogueResonanceEvent
    {
        public readonly int Points;
        public DialogueResonanceEvent(int points) => Points = points;
    }
}
