namespace Amane.Time
{
    /// <summary>
    /// 1日の時間帯。DESIGN.md「時間システム」に対応。
    /// 自由行動が可能なのは Lunch / AfterSchool / Evening の最大3AP（昼休みは校内限定）。
    /// LateNight は潜行専用（潜行するとその日のAPを全消費）。
    /// </summary>
    public enum TimeSlot
    {
        Morning,      // 登校前（固定演出）
        Class,        // 授業中（内面ステータス上昇チャンス）
        Lunch,        // 昼休み（校内限定 AP1・潜行不可）
        AfterSchool,  // 放課後（自由行動 AP1）
        Evening,      // 夜（自由行動 AP1）
        LateNight     // 深夜（潜行 or 自室）
    }

    /// <summary>天候。霧の日は澱が活性化する。</summary>
    public enum Weather
    {
        Clear,  // 晴
        Rain,   // 雨
        Fog     // 霧（潜行報酬増・難度増）
    }
}
