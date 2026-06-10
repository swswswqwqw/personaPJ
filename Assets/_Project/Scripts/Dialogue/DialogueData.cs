using System;
using System.Collections.Generic;

namespace Amane.Dialogue
{
    [Serializable]
    public sealed class DialogueData
    {
        public string id;
        public string title;
        public List<DialogueLine> lines = new();

        public string bondId;
        public int bondPointsOnComplete;

        public List<DialogueChoice> choices;
    }

    [Serializable]
    public sealed class DialogueChoice
    {
        public int afterLineIndex;
        public List<DialogueOption> options = new();
    }

    [Serializable]
    public sealed class DialogueOption
    {
        public string text;
        public string requiredStat;
        public int requiredRank;
        public int bondBonus;
        public string jumpToDialogueId;
    }
}
