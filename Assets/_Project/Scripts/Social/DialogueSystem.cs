using UnityEngine;
using System;
using System.Collections.Generic;
using Astra.Core;

namespace Astra.Social
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public string expression;
        public string voiceKey;
        public float delay;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public string responseKey;
        public int resonancePoints;
        public string statBoost;
        public int statAmount;
    }

    [Serializable]
    public class DialogueNode
    {
        public string id;
        public DialogueLine[] lines;
        public DialogueChoice[] choices;
        public string nextNodeId;
    }

    [Serializable]
    public class DialogueData
    {
        public string conversationId;
        public string characterName;
        public int requiredRank;
        public DialogueNode[] nodes;
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        private DialogueData _currentDialogue;
        private int _currentNodeIndex;
        private int _currentLineIndex;
        private bool _isActive;

        public bool IsActive => _isActive;
        public DialogueLine CurrentLine
        {
            get
            {
                if (_currentDialogue == null) return null;
                var node = _currentDialogue.nodes[_currentNodeIndex];
                if (_currentLineIndex >= node.lines.Length) return null;
                return node.lines[_currentLineIndex];
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartDialogue(DialogueData data)
        {
            _currentDialogue = data;
            _currentNodeIndex = 0;
            _currentLineIndex = 0;
            _isActive = true;

            GameManager.Instance?.ChangePhase(GamePhase.Dialogue);
            EventBus.Publish(new DialogueStartedEvent(data.conversationId));
            EventBus.Publish(new DialogueLineEvent(CurrentLine));
        }

        public void AdvanceLine()
        {
            if (!_isActive || _currentDialogue == null) return;

            var node = _currentDialogue.nodes[_currentNodeIndex];
            _currentLineIndex++;

            if (_currentLineIndex >= node.lines.Length)
            {
                if (node.choices != null && node.choices.Length > 0)
                {
                    EventBus.Publish(new DialogueChoicesEvent(node.choices));
                    return;
                }
                AdvanceNode(node.nextNodeId);
                return;
            }

            EventBus.Publish(new DialogueLineEvent(CurrentLine));
        }

        public void SelectChoice(int choiceIndex)
        {
            var node = _currentDialogue.nodes[_currentNodeIndex];
            if (node.choices == null || choiceIndex >= node.choices.Length) return;

            var choice = node.choices[choiceIndex];
            EventBus.Publish(new DialogueChoiceSelectedEvent(choice));

            AdvanceNode(choice.responseKey);
        }

        private void AdvanceNode(string nextNodeId)
        {
            if (string.IsNullOrEmpty(nextNodeId))
            {
                EndDialogue();
                return;
            }

            for (int i = 0; i < _currentDialogue.nodes.Length; i++)
            {
                if (_currentDialogue.nodes[i].id == nextNodeId)
                {
                    _currentNodeIndex = i;
                    _currentLineIndex = 0;
                    EventBus.Publish(new DialogueLineEvent(CurrentLine));
                    return;
                }
            }

            EndDialogue();
        }

        private void EndDialogue()
        {
            _isActive = false;
            var id = _currentDialogue?.conversationId;
            _currentDialogue = null;
            EventBus.Publish(new DialogueEndedEvent(id));
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public readonly struct DialogueStartedEvent
    {
        public readonly string ConversationId;
        public DialogueStartedEvent(string id) { ConversationId = id; }
    }
    public readonly struct DialogueLineEvent
    {
        public readonly DialogueLine Line;
        public DialogueLineEvent(DialogueLine line) { Line = line; }
    }
    public readonly struct DialogueChoicesEvent
    {
        public readonly DialogueChoice[] Choices;
        public DialogueChoicesEvent(DialogueChoice[] choices) { Choices = choices; }
    }
    public readonly struct DialogueChoiceSelectedEvent
    {
        public readonly DialogueChoice Choice;
        public DialogueChoiceSelectedEvent(DialogueChoice choice) { Choice = choice; }
    }
    public readonly struct DialogueEndedEvent
    {
        public readonly string ConversationId;
        public DialogueEndedEvent(string id) { ConversationId = id; }
    }
}
