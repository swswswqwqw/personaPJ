using System;
using System.Collections.Generic;

namespace Amane.Time
{
    public enum CalendarEventType
    {
        NewCase,         // 月初の新事件
        BondMilestone,   // 絆ランク到達イベント
        FullMoon,        // 満月の夜（メインストーリー進行）
        ExamPeriod,      // テスト期間
        DeadlineWarning, // デッドライン3日前
        SeasonalEvent,   // 学園祭・大晦日等の限定イベント
        StoryBeat        // メインストーリーの固定イベント
    }

    public sealed class CalendarEvent
    {
        public string Id { get; }
        public string DisplayName { get; }
        public CalendarEventType Type { get; }
        public GameDate TriggerDate { get; }
        public bool Triggered { get; private set; }

        public CalendarEvent(string id, string displayName, CalendarEventType type, GameDate triggerDate)
        {
            Id = id;
            DisplayName = displayName;
            Type = type;
            TriggerDate = triggerDate;
        }

        public bool ShouldTrigger(GameDate today)
        {
            return !Triggered && today.DayIndex >= TriggerDate.DayIndex;
        }

        public void MarkTriggered() => Triggered = true;
    }

    public sealed class CalendarEventScheduler
    {
        private readonly List<CalendarEvent> _events = new();

        public void Register(CalendarEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            _events.Add(evt);
        }

        public List<CalendarEvent> GetPendingEvents(GameDate today)
        {
            var pending = new List<CalendarEvent>();
            foreach (var evt in _events)
            {
                if (evt.ShouldTrigger(today))
                    pending.Add(evt);
            }
            return pending;
        }

        public void SeedStoryEvents()
        {
            // 第1幕: 4月〜7月
            Register(new CalendarEvent("awakening", "残響同調の覚醒", CalendarEventType.StoryBeat, new GameDate(3)));
            Register(new CalendarEvent("fullmoon_apr", "満月——母の記憶①", CalendarEventType.FullMoon, new GameDate(15)));
            Register(new CalendarEvent("case_mizuki_start", "美月の心象が開く", CalendarEventType.NewCase, new GameDate(31)));
            Register(new CalendarEvent("midterm_june", "中間テスト", CalendarEventType.ExamPeriod, new GameDate(70)));
            Register(new CalendarEvent("fullmoon_jun", "満月——母の記憶②", CalendarEventType.FullMoon, new GameDate(76)));
            Register(new CalendarEvent("case_ritsu_start", "律の事件発生", CalendarEventType.NewCase, new GameDate(62)));
            Register(new CalendarEvent("case_ren_start", "蓮の事件発生", CalendarEventType.NewCase, new GameDate(92)));
            Register(new CalendarEvent("fullmoon_jul", "満月——母の記憶③", CalendarEventType.FullMoon, new GameDate(107)));
            Register(new CalendarEvent("end_act1", "終業式・第1幕クライマックス", CalendarEventType.StoryBeat, new GameDate(122)));

            // 第2幕: 9月〜12月
            Register(new CalendarEvent("case_nagisa_start", "渚の失踪", CalendarEventType.NewCase, new GameDate(153)));
            Register(new CalendarEvent("festival_prep", "学園祭準備", CalendarEventType.SeasonalEvent, new GameDate(170)));
            Register(new CalendarEvent("twist_1", "どんでん返し①——封筒の真実", CalendarEventType.StoryBeat, new GameDate(183)));
            Register(new CalendarEvent("power_runaway", "力の暴走", CalendarEventType.StoryBeat, new GameDate(214)));
            Register(new CalendarEvent("new_year_eve", "大晦日イベント", CalendarEventType.SeasonalEvent, new GameDate(275)));

            // 第3幕: 1月〜3月
            Register(new CalendarEvent("twist_2", "どんでん返し②——黒幕の正体", CalendarEventType.StoryBeat, new GameDate(306)));
            Register(new CalendarEvent("final_dungeon", "言伝石の最奥開放", CalendarEventType.StoryBeat, new GameDate(337)));
            Register(new CalendarEvent("graduation", "卒業・最終決戦", CalendarEventType.StoryBeat, new GameDate(365)));
        }

        public IReadOnlyList<CalendarEvent> AllEvents => _events;
    }
}
