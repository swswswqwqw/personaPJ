using UnityEngine;
using ArcadiaOfEchoes.Time;

namespace ArcadiaOfEchoes.Data
{
    public enum CalendarEventType
    {
        Story,         // メインストーリー
        BossDeadline,  // ボス戦期限
        SchoolExam,    // 試験
        Festival,      // 文化祭等
        Holiday,       // 祝日・長期休暇
        Special        // 特殊イベント（残響時など）
    }

    [CreateAssetMenu(fileName = "NewCalendarEvent", menuName = "ArcadiaOfEchoes/Calendar Event")]
    public class CalendarEventData : ScriptableObject
    {
        [Header("イベント基本情報")]
        public string EventId;
        public string DisplayName;
        [TextArea(2, 4)] public string Description;
        public CalendarEventType EventType;

        [Header("日程")]
        public GameDate TriggerDate;
        public TimePeriod TriggerPeriod;
        public GameDate DeadlineDate;

        [Header("条件")]
        public bool IsRepeating;
        public string RequiredFlag;
        public int RequiredBondRank;
        public string RequiredBondCharacterId;

        [Header("シーン")]
        public string SceneToLoad;
        public string DialogueId;
    }
}
