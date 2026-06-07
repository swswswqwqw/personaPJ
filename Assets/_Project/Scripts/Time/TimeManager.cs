using System;
using System.Collections.Generic;
using Amane.Core;

namespace Amane.Time
{
    /// <summary>
    /// ゲーム内時間・カレンダー・AP・天候・デッドラインを管理する Plain C# クラス。
    /// MonoBehaviour に依存しない。生成は GameManager が行い、EventChannel 経由で通知する。
    /// テーマ「時間の希少性」の中核。1日の自由APは最大2。潜行で全消費。
    /// </summary>
    public sealed class TimeManager
    {
        public const int MaxActionPoints = 2;

        private readonly EventChannel _events;
        private readonly List<Deadline> _deadlines = new();
        private readonly Func<GameDate, Weather> _weatherResolver;

        public GameDate Today { get; private set; }
        public TimeSlot CurrentSlot { get; private set; }
        public Weather TodayWeather { get; private set; }
        public int ActionPoints { get; private set; }

        public IReadOnlyList<Deadline> Deadlines => _deadlines;

        /// <param name="weatherResolver">日付→天候の決定関数（注入可能・テスト容易）。</param>
        public TimeManager(EventChannel events, Func<GameDate, Weather> weatherResolver = null)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _weatherResolver = weatherResolver ?? DefaultWeather;
            Today = new GameDate(1); // 4/1
            CurrentSlot = TimeSlot.Morning;
            TodayWeather = _weatherResolver(Today);
            ActionPoints = MaxActionPoints;
        }

        public void RegisterDeadline(Deadline deadline)
        {
            if (deadline == null) throw new ArgumentNullException(nameof(deadline));
            _deadlines.Add(deadline);
        }

        /// <summary>次の時間帯へ進める。日をまたぐ場合は AdvanceDay を呼ぶ。</summary>
        public void AdvanceSlot()
        {
            if (CurrentSlot == TimeSlot.LateNight)
            {
                AdvanceDay();
                return;
            }
            CurrentSlot = (TimeSlot)((int)CurrentSlot + 1);
            _events.Publish(new TimeAdvancedEvent(Today.DayIndex, CurrentSlot));
            CheckDeadlines();
        }

        /// <summary>自由行動でAPを1消費する。消費できたら true。</summary>
        public bool SpendActionPoint()
        {
            if (ActionPoints <= 0) return false;
            ActionPoints--;
            return true;
        }

        /// <summary>未言界へ潜行する。その日のAPを全消費し深夜まで進める。</summary>
        public bool Dive()
        {
            if (ActionPoints <= 0) return false;
            ActionPoints = 0;
            CurrentSlot = TimeSlot.LateNight;
            _events.Publish(new TimeAdvancedEvent(Today.DayIndex, CurrentSlot));
            return true;
        }

        /// <summary>翌日へ。AP回復・天候更新・デッドライン判定。学年末で停止。</summary>
        public void AdvanceDay()
        {
            if (Today.IsLastDay) return; // 学年末。上位がエンディング処理へ。
            Today = Today.AddDays(1);
            CurrentSlot = TimeSlot.Morning;
            ActionPoints = MaxActionPoints;
            TodayWeather = _weatherResolver(Today);
            _events.Publish(new DayChangedEvent(Today.DayIndex, TodayWeather));
            CheckDeadlines();
        }

        private void CheckDeadlines()
        {
            foreach (var d in _deadlines)
            {
                if (d.Cleared) continue;
                int left = d.DaysLeft(Today);
                // 期限3日前以内で接近通知（DESIGN.md: 予告状構造の前段演出）。
                if (left >= 0 && left <= 3)
                    _events.Publish(new DeadlineApproachingEvent(d.CaseId, left));
            }
        }

        /// <summary>未クリアのまま期限切れになったデッドラインを返す（バッドフラグ用）。</summary>
        public IEnumerable<Deadline> GetFailedDeadlines()
        {
            foreach (var d in _deadlines)
                if (d.IsFailed(Today)) yield return d;
        }

        // 決定論的な仮天候: 通日ベースの簡易パターン（後でデータ駆動へ差し替え）。
        private static Weather DefaultWeather(GameDate date)
        {
            int n = date.DayIndex % 5;
            return n switch
            {
                0 => Weather.Fog,
                1 => Weather.Rain,
                _ => Weather.Clear
            };
        }
    }
}
