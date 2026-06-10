using System;
using System.Collections.Generic;

namespace Amane.Core
{
    [Serializable]
    public sealed class SaveData
    {
        public int dayIndex;
        public int currentSlot;
        public int actionPoints;

        public List<StatEntry> innerStats = new();
        public List<BondEntry> bonds = new();
        public List<string> clearedDeadlines = new();

        public string timestamp;
    }

    [Serializable]
    public sealed class StatEntry
    {
        public string stat;
        public int points;
    }

    [Serializable]
    public sealed class BondEntry
    {
        public string id;
        public int rank;
        public int points;
    }
}
