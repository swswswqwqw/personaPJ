using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadiaOfEchoes.Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        public bool IsDialogueActive { get; private set; }
        public DialogueNode CurrentNode { get; private set; }

        public event Action<DialogueNode> OnDialogueNodeStarted;
        public event Action<DialogueChoice> OnChoiceMade;
        public event Action OnDialogueEnded;

        private DialogueSequence _currentSequence;
        private int _currentNodeIndex;

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

        public void StartDialogue(DialogueSequence sequence)
        {
            _currentSequence = sequence;
            _currentNodeIndex = 0;
            IsDialogueActive = true;
            ShowNode(_currentSequence.Nodes[0]);
        }

        public void AdvanceDialogue()
        {
            if (!IsDialogueActive) return;
            if (CurrentNode.HasChoices) return;

            _currentNodeIndex++;
            if (_currentNodeIndex >= _currentSequence.Nodes.Count)
            {
                EndDialogue();
                return;
            }
            ShowNode(_currentSequence.Nodes[_currentNodeIndex]);
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!IsDialogueActive || !CurrentNode.HasChoices) return;
            if (choiceIndex < 0 || choiceIndex >= CurrentNode.Choices.Count) return;

            var choice = CurrentNode.Choices[choiceIndex];
            OnChoiceMade?.Invoke(choice);

            if (!string.IsNullOrEmpty(choice.JumpToNodeId))
            {
                var targetNode = _currentSequence.Nodes.Find(n => n.NodeId == choice.JumpToNodeId);
                if (targetNode != null)
                {
                    _currentNodeIndex = _currentSequence.Nodes.IndexOf(targetNode);
                    ShowNode(targetNode);
                    return;
                }
            }

            AdvanceDialogue();
        }

        public void EndDialogue()
        {
            IsDialogueActive = false;
            CurrentNode = null;
            _currentSequence = null;
            OnDialogueEnded?.Invoke();
        }

        private void ShowNode(DialogueNode node)
        {
            CurrentNode = node;
            OnDialogueNodeStarted?.Invoke(node);
        }
    }

    [Serializable]
    public class DialogueSequence
    {
        public string SequenceId;
        public string ContextDescription;
        public List<DialogueNode> Nodes = new();
    }

    [Serializable]
    public class DialogueNode
    {
        public string NodeId;
        public string SpeakerName;
        public string SpeakerId;
        [TextArea] public string Text;
        public string Emotion;
        public List<DialogueChoice> Choices = new();
        public bool HasChoices => Choices != null && Choices.Count > 0;
    }

    [Serializable]
    public class DialogueChoice
    {
        [TextArea] public string ChoiceText;
        public string JumpToNodeId;
        public int ResonancePoints;
        public string RequiredStat;
        public int RequiredStatRank;
    }
}
