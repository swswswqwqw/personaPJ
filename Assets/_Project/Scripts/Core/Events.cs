using AstralEchoes.Data;

namespace AstralEchoes.Core
{
    public struct TimeAdvancedEvent
    {
        public TimeOfDay PreviousTime;
        public TimeOfDay NewTime;
        public int Day;
        public int Month;
    }

    public struct DayAdvancedEvent
    {
        public int PreviousDay;
        public int NewDay;
        public int Month;
    }

    public struct ActionPointsChangedEvent
    {
        public float Remaining;
        public float Used;
    }

    public struct BattleStartedEvent
    {
        public string EncounterId;
    }

    public struct BattleEndedEvent
    {
        public bool Victory;
        public int ExpGained;
        public int MoneyGained;
    }

    public struct AttunementRankUpEvent
    {
        public string CharacterId;
        public Arcana CharacterArcana;
        public int NewRank;
    }

    public struct InnerFrequencyChangedEvent
    {
        public InnerFrequency Stat;
        public int PreviousValue;
        public int NewValue;
    }

    public struct WeaknessHitEvent
    {
        public string AttackerId;
        public string TargetId;
        public Attribute AttributeUsed;
    }

    public struct FullResonanceReadyEvent
    {
        public int DownedEnemyCount;
    }

    public struct DialogueStartedEvent
    {
        public string DialogueId;
        public string SpeakerId;
    }

    public struct DialogueEndedEvent
    {
        public string DialogueId;
    }

    public struct SceneTransitionEvent
    {
        public string FromScene;
        public string ToScene;
    }
}
