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
        public string expression;
        [TextArea(2, 5)] public string text;
        public List<DialogueChoice> choices;
        public string nextLineId;
        public bool autoAdvance;
        public float autoAdvanceDelay = 2f;
    }

    [Serializable]
    public class DialogueChoice
    {
        [TextArea(1, 2)] public string text;
        public string nextLineId;
        public int bondPoints;
        public Data.PersonalStat? statRequirement;
        public Data.StatRank requiredRank;
        public Data.PersonalStat? statReward;
        public int statRewardPoints;
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "EchoesOfArcadia/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;
        [TextArea(1, 3)] public string contextDescription;
        public Data.Arcana relatedArcana;
        public int requiredBondRank;
        public List<DialogueLine> lines;
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        private DialogueData currentDialogue;
        private int currentLineIndex;
        private bool isActive;

        public bool IsActive => isActive;
        public DialogueLine CurrentLine => isActive && currentLineIndex < currentDialogue.lines.Count
            ? currentDialogue.lines[currentLineIndex]
            : null;

        public event Action<DialogueLine> OnLineDisplayed;
        public event Action<List<DialogueChoice>> OnChoicesPresented;
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

        public void StartDialogue(DialogueData dialogue)
        {
            currentDialogue = dialogue;
            currentLineIndex = 0;
            isActive = true;

            GameManager.Instance?.ChangePhase(GamePhase.Dialogue);
            DisplayCurrentLine();
        }

        public void AdvanceLine()
        {
            if (!isActive) return;

            var current = CurrentLine;
            if (current?.choices != null && current.choices.Count > 0) return;

            currentLineIndex++;

            if (currentLineIndex >= currentDialogue.lines.Count)
            {
                EndDialogue();
                return;
            }

            DisplayCurrentLine();
        }

        public void SelectChoice(int choiceIndex)
        {
            if (!isActive) return;
            var current = CurrentLine;
            if (current?.choices == null || choiceIndex >= current.choices.Count) return;

            var choice = current.choices[choiceIndex];

            if (choice.bondPoints > 0 && currentDialogue.relatedArcana != Data.Arcana.Fool)
            {
                GameEventBus.Publish(new DialogueChoiceEvent(
                    currentDialogue.relatedArcana, choice.bondPoints));
            }

            if (choice.statReward.HasValue && choice.statRewardPoints > 0)
            {
                Data.PlayerStats.Instance?.AddPoints(choice.statReward.Value, choice.statRewardPoints);
            }

            if (!string.IsNullOrEmpty(choice.nextLineId))
            {
                JumpToLine(choice.nextLineId);
            }
            else
            {
                currentLineIndex++;
                if (currentLineIndex >= currentDialogue.lines.Count)
                {
                    EndDialogue();
                    return;
                }
                DisplayCurrentLine();
            }
        }

        private void DisplayCurrentLine()
        {
            var line = CurrentLine;
            if (line == null) return;

            OnLineDisplayed?.Invoke(line);

            if (line.choices != null && line.choices.Count > 0)
            {
                OnChoicesPresented?.Invoke(line.choices);
            }
        }

        private void JumpToLine(string lineId)
        {
            for (int i = 0; i < currentDialogue.lines.Count; i++)
            {
                if (currentDialogue.lines[i].speakerName == lineId ||
                    i.ToString() == lineId)
                {
                    currentLineIndex = i;
                    DisplayCurrentLine();
                    return;
                }
            }

            if (int.TryParse(lineId, out int index) && index < currentDialogue.lines.Count)
            {
                currentLineIndex = index;
                DisplayCurrentLine();
                return;
            }

            currentLineIndex++;
            if (currentLineIndex >= currentDialogue.lines.Count)
                EndDialogue();
            else
                DisplayCurrentLine();
        }

        private void EndDialogue()
        {
            isActive = false;
            currentDialogue = null;
            OnDialogueEnded?.Invoke();
        }
    }

    public readonly struct DialogueChoiceEvent
    {
        public readonly Data.Arcana Arcana;
        public readonly int BondPoints;

        public DialogueChoiceEvent(Data.Arcana arcana, int bondPoints)
        {
            Arcana = arcana;
            BondPoints = bondPoints;
        }
    }
}
