using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfBacklight.Core;

namespace AriaOfBacklight.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public bool IsActive { get; private set; }
        public DialogueLine CurrentLine { get; private set; }

        private Queue<DialogueLine> lineQueue = new();
        private Action onDialogueComplete;

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

        public void StartDialogue(DialogueScript script, Action onComplete = null)
        {
            lineQueue.Clear();
            foreach (var line in script.lines)
                lineQueue.Enqueue(line);

            onDialogueComplete = onComplete;
            IsActive = true;
            ShowNextLine();
        }

        public void ShowNextLine()
        {
            if (lineQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            CurrentLine = lineQueue.Dequeue();

            if (CurrentLine.hasChoices)
            {
                EventBus.Publish(new DialogueChoiceEvent(CurrentLine.choices));
            }
            else
            {
                EventBus.Publish(new DialogueLineEvent(CurrentLine));
            }
        }

        public void SelectChoice(int index)
        {
            if (CurrentLine?.choices == null || index >= CurrentLine.choices.Length) return;

            var choice = CurrentLine.choices[index];
            EventBus.Publish(new DialogueChoiceSelectedEvent(index, choice));

            if (!string.IsNullOrEmpty(choice.bondCharacterId))
            {
                Social.BondManager.Instance?.AddPoints(choice.bondCharacterId, choice.bondPointsReward);
            }

            ShowNextLine();
        }

        private void EndDialogue()
        {
            IsActive = false;
            CurrentLine = null;
            EventBus.Publish(new DialogueEndedEvent());
            onDialogueComplete?.Invoke();
        }
    }

    public readonly struct DialogueLineEvent
    {
        public readonly DialogueLine Line;
        public DialogueLineEvent(DialogueLine line) { Line = line; }
    }

    public readonly struct DialogueChoiceEvent
    {
        public readonly DialogueChoice[] Choices;
        public DialogueChoiceEvent(DialogueChoice[] choices) { Choices = choices; }
    }

    public readonly struct DialogueChoiceSelectedEvent
    {
        public readonly int Index;
        public readonly DialogueChoice Choice;
        public DialogueChoiceSelectedEvent(int index, DialogueChoice choice) { Index = index; Choice = choice; }
    }

    public readonly struct DialogueEndedEvent { }
}
