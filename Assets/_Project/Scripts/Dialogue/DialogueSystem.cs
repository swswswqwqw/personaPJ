using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfEchoes.Core;

namespace AriaOfEchoes.Dialogue
{
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string speakerId;
        [TextArea(2, 5)] public string text;
        public string expression;
        public float delay;
        public List<DialogueChoice> choices;
        public bool hasChoices => choices != null && choices.Count > 0;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public string nextNodeId;
        public int bondPoints;
        public string requiredStat;
        public int requiredStatRank;
    }

    [Serializable]
    public class DialogueNode
    {
        public string nodeId;
        public List<DialogueLine> lines = new();
        public string nextNodeId;
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "AriaOfEchoes/DialogueData")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;
        public string title;
        public List<DialogueNode> nodes = new();

        public DialogueNode GetNode(string nodeId)
        {
            return nodes.Find(n => n.nodeId == nodeId);
        }

        public DialogueNode GetFirstNode()
        {
            return nodes.Count > 0 ? nodes[0] : null;
        }
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        DialogueData currentDialogue;
        DialogueNode currentNode;
        int currentLineIndex;

        public bool IsActive { get; private set; }
        public DialogueLine CurrentLine => currentNode?.lines[currentLineIndex];

        public event Action<DialogueLine> OnLineDisplayed;
        public event Action<List<DialogueChoice>> OnChoicesDisplayed;
        public event Action OnDialogueEnded;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartDialogue(DialogueData dialogue)
        {
            currentDialogue = dialogue;
            currentNode = dialogue.GetFirstNode();
            currentLineIndex = 0;
            IsActive = true;

            EventBus.Publish(new DialogueStartedEvent(dialogue.dialogueId));
            DisplayCurrentLine();
        }

        public void AdvanceLine()
        {
            if (!IsActive || currentNode == null) return;

            if (CurrentLine.hasChoices)
                return;

            currentLineIndex++;

            if (currentLineIndex >= currentNode.lines.Count)
            {
                if (!string.IsNullOrEmpty(currentNode.nextNodeId))
                {
                    currentNode = currentDialogue.GetNode(currentNode.nextNodeId);
                    currentLineIndex = 0;
                    DisplayCurrentLine();
                }
                else
                {
                    EndDialogue();
                }
                return;
            }

            DisplayCurrentLine();
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!IsActive || CurrentLine == null || !CurrentLine.hasChoices) return;

            var choice = CurrentLine.choices[choiceIndex];

            EventBus.Publish(new DialogueChoiceEvent(
                currentDialogue.dialogueId, choice.text, choice.bondPoints));

            if (!string.IsNullOrEmpty(choice.nextNodeId))
            {
                currentNode = currentDialogue.GetNode(choice.nextNodeId);
                currentLineIndex = 0;
                DisplayCurrentLine();
            }
            else
            {
                AdvanceLine();
            }
        }

        void DisplayCurrentLine()
        {
            if (currentNode == null || currentLineIndex >= currentNode.lines.Count)
            {
                EndDialogue();
                return;
            }

            var line = CurrentLine;
            OnLineDisplayed?.Invoke(line);
            EventBus.Publish(new DialogueLineEvent(line.speakerName, line.text));

            if (line.hasChoices)
                OnChoicesDisplayed?.Invoke(line.choices);
        }

        void EndDialogue()
        {
            IsActive = false;
            EventBus.Publish(new DialogueEndedEvent(currentDialogue.dialogueId));
            OnDialogueEnded?.Invoke();
            currentDialogue = null;
            currentNode = null;
        }
    }

    public struct DialogueStartedEvent
    {
        public string DialogueId;
        public DialogueStartedEvent(string id) { DialogueId = id; }
    }

    public struct DialogueEndedEvent
    {
        public string DialogueId;
        public DialogueEndedEvent(string id) { DialogueId = id; }
    }

    public struct DialogueLineEvent
    {
        public string Speaker;
        public string Text;
        public DialogueLineEvent(string s, string t) { Speaker = s; Text = t; }
    }

    public struct DialogueChoiceEvent
    {
        public string DialogueId;
        public string ChoiceText;
        public int BondPoints;
        public DialogueChoiceEvent(string id, string text, int points)
        { DialogueId = id; ChoiceText = text; BondPoints = points; }
    }
}
