using System;
using UnityEngine;

namespace AriaOfBacklight.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "AriaOfBacklight/DialogueScript")]
    public class DialogueScript : ScriptableObject
    {
        public string dialogueId;
        public DialogueLine[] lines;
    }

    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string speakerId;
        [TextArea(2, 5)] public string text;
        public string emotion;
        public bool hasChoices;
        public DialogueChoice[] choices;
        public float pauseBefore;
        public string animationTrigger;
    }

    [Serializable]
    public class DialogueChoice
    {
        [TextArea(1, 2)] public string text;
        public string bondCharacterId;
        public int bondPointsReward;
        public string nextDialogueId;
        public Social.PlayerStatType? requiredStat;
        public int requiredStatLevel;
    }
}
