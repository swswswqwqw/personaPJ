using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ArcanaOfHollows.Core;

namespace ArcanaOfHollows.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private float textSpeed = 0.03f;

        public bool IsDialogueActive { get; private set; }

        private DialogueSequence currentSequence;
        private int currentLineIndex;

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

        public async Task PlayDialogue(DialogueSequence sequence)
        {
            if (IsDialogueActive) return;

            IsDialogueActive = true;
            currentSequence = sequence;
            currentLineIndex = 0;

            EventBus.Publish(new DialogueStartedEvent(sequence.sequenceId));

            while (currentLineIndex < sequence.lines.Count)
            {
                var line = sequence.lines[currentLineIndex];
                await DisplayLine(line);
                currentLineIndex++;
            }

            IsDialogueActive = false;
            EventBus.Publish(new DialogueEndedEvent(sequence.sequenceId));
        }

        private async Task DisplayLine(DialogueLine line)
        {
            EventBus.Publish(new DialogueLineEvent(line));

            if (line.choices != null && line.choices.Count > 0)
            {
                EventBus.Publish(new DialogueChoicesEvent(line.choices));
                await WaitForChoiceSelection();
                return;
            }

            await TypewriterEffect(line.text);
            await WaitForInput();
        }

        private async Task TypewriterEffect(string text)
        {
            var displayedText = "";
            foreach (char c in text)
            {
                displayedText += c;
                EventBus.Publish(new DialogueTextUpdateEvent(displayedText));
                await Task.Delay((int)(textSpeed * 1000));
            }
        }

        private async Task WaitForInput()
        {
            while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space))
                await Task.Yield();
        }

        private async Task WaitForChoiceSelection()
        {
            selectedChoiceIndex = -1;
            while (selectedChoiceIndex < 0)
                await Task.Yield();
        }

        private int selectedChoiceIndex = -1;

        public void SelectChoice(int index)
        {
            if (!IsDialogueActive) return;
            selectedChoiceIndex = index;

            var line = currentSequence.lines[currentLineIndex];
            if (index >= 0 && index < line.choices.Count)
            {
                var choice = line.choices[index];
                EventBus.Publish(new DialogueChoiceSelectedEvent(choice));
            }
        }
    }

    [System.Serializable]
    public class DialogueSequence
    {
        public string sequenceId;
        public List<DialogueLine> lines = new();
    }

    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string speakerId;
        public string text;
        public string emotion;
        public List<DialogueChoice> choices;
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string text;
        public string nextSequenceId;
        public int heartStringPoints;
        public string requiredStat;
        public int requiredStatLevel;
    }

    public readonly struct DialogueStartedEvent
    {
        public readonly string SequenceId;
        public DialogueStartedEvent(string id) { SequenceId = id; }
    }

    public readonly struct DialogueEndedEvent
    {
        public readonly string SequenceId;
        public DialogueEndedEvent(string id) { SequenceId = id; }
    }

    public readonly struct DialogueLineEvent
    {
        public readonly DialogueLine Line;
        public DialogueLineEvent(DialogueLine line) { Line = line; }
    }

    public readonly struct DialogueTextUpdateEvent
    {
        public readonly string DisplayedText;
        public DialogueTextUpdateEvent(string text) { DisplayedText = text; }
    }

    public readonly struct DialogueChoicesEvent
    {
        public readonly List<DialogueChoice> Choices;
        public DialogueChoicesEvent(List<DialogueChoice> choices) { Choices = choices; }
    }

    public readonly struct DialogueChoiceSelectedEvent
    {
        public readonly DialogueChoice Choice;
        public DialogueChoiceSelectedEvent(DialogueChoice choice) { Choice = choice; }
    }
}
