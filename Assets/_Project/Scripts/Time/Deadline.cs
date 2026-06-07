namespace Amane.Time
{
    /// <summary>
    /// 月次デッドライン（ボス戦期限）。DESIGN.md の各月事件に対応。
    /// 期限内に対象の心象世界最奥へ到達できないと対象者が廃人化する。
    /// </summary>
    public sealed class Deadline
    {
        public string CaseId { get; }
        public string DisplayName { get; }
        public GameDate DueDate { get; }
        public bool Cleared { get; private set; }

        public Deadline(string caseId, string displayName, GameDate dueDate)
        {
            CaseId = caseId;
            DisplayName = displayName;
            DueDate = dueDate;
        }

        public void MarkCleared() => Cleared = true;

        /// <summary>現在日からの残り日数。負ならば期限切れ。</summary>
        public int DaysLeft(GameDate today) => DueDate.DayIndex - today.DayIndex;

        /// <summary>期限切れかつ未クリア＝失敗確定。</summary>
        public bool IsFailed(GameDate today) => !Cleared && DaysLeft(today) < 0;
    }
}
