using System;
using System.Collections.Generic;
using AstralEchoes.Core;
using AstralEchoes.Data;

namespace AstralEchoes.Dialogue
{
    public sealed class DialogueManager
    {
        static DialogueManager _instance;
        public static DialogueManager Instance => _instance ??= new DialogueManager();

        public bool IsActive { get; private set; }
        public DialogueNode CurrentNode { get; private set; }

        DialogueSequence _currentSequence;
        int _nodeIndex;

        public event Action<DialogueNode> OnNodeDisplayed;
        public event Action<List<DialogueChoice>> OnChoicesPresented;
        public event Action OnDialogueEnded;

        DialogueManager() { }

        public void StartDialogue(DialogueSequence sequence)
        {
            if (IsActive) return;

            _currentSequence = sequence;
            _nodeIndex = 0;
            IsActive = true;

            GameManager.Instance.ChangeState(GameState.Dialogue);
            EventBus.Publish(new DialogueStartedEvent
            {
                DialogueId = sequence.Id,
                SpeakerId = sequence.Nodes.Count > 0 ? sequence.Nodes[0].SpeakerId : ""
            });

            DisplayCurrentNode();
        }

        public void Advance()
        {
            if (!IsActive) return;

            var node = _currentSequence.Nodes[_nodeIndex];

            if (node.Choices != null && node.Choices.Count > 0)
                return;

            _nodeIndex++;
            if (_nodeIndex >= _currentSequence.Nodes.Count)
            {
                EndDialogue();
                return;
            }

            DisplayCurrentNode();
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!IsActive) return;
            var node = _currentSequence.Nodes[_nodeIndex];
            if (node.Choices == null || choiceIndex >= node.Choices.Count) return;

            var choice = node.Choices[choiceIndex];

            choice.OnSelected?.Invoke();

            if (!string.IsNullOrEmpty(choice.JumpToNodeId))
            {
                int jumpIndex = _currentSequence.Nodes.FindIndex(n => n.Id == choice.JumpToNodeId);
                if (jumpIndex >= 0)
                {
                    _nodeIndex = jumpIndex;
                    DisplayCurrentNode();
                    return;
                }
            }

            _nodeIndex++;
            if (_nodeIndex >= _currentSequence.Nodes.Count)
            {
                EndDialogue();
                return;
            }

            DisplayCurrentNode();
        }

        void DisplayCurrentNode()
        {
            CurrentNode = _currentSequence.Nodes[_nodeIndex];
            OnNodeDisplayed?.Invoke(CurrentNode);

            if (CurrentNode.Choices != null && CurrentNode.Choices.Count > 0)
                OnChoicesPresented?.Invoke(CurrentNode.Choices);
        }

        void EndDialogue()
        {
            IsActive = false;
            CurrentNode = null;

            EventBus.Publish(new DialogueEndedEvent
            {
                DialogueId = _currentSequence.Id
            });

            OnDialogueEnded?.Invoke();
            _currentSequence = null;
        }
    }

    [Serializable]
    public class DialogueSequence
    {
        public string Id;
        public List<DialogueNode> Nodes = new();
    }

    [Serializable]
    public class DialogueNode
    {
        public string Id;
        public string SpeakerId;
        public string SpeakerNameJP;
        public string Text;
        public string Expression;
        public bool IsImportant;
        public List<DialogueChoice> Choices;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string Text;
        public string JumpToNodeId;
        public int AttunementPoints;
        public string RequiredStat;
        public int RequiredStatRank;
        public Action OnSelected;
    }
}
