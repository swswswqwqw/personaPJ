namespace ArcadiaOfEchoes.Battle
{
    public enum ElementType
    {
        Resonance,   // 共鳴 — 共感・理解
        Dissonance,  // 不協和 — 怒り・拒絶
        Silence,     // 静寂 — 悲しみ・諦め
        Roar,        // 咆哮 — 激情・衝動
        Echo,        // 残響 — 記憶・郷愁
        Void,        // 虚無 — 空虚・喪失
        Impact,      // 物理
        Heal         // 治癒
    }

    public enum AffinityType
    {
        Normal,
        Weak,      // 弱点 — ワンモア・エコー発動
        Resist,    // 耐性 — ダメージ半減
        Null,      // 無効
        Absorb,    // 吸収
        Reflect    // 反射
    }

    public enum BattleAction
    {
        Attack,
        Skill,
        Guard,
        Item,
        EchoRelay, // バトンパス相当
        Escape
    }

    public enum BattlerState
    {
        Normal,
        Down,      // ダウン — 弱点被弾後
        Guard,
        Dead
    }

    public enum TurnResult
    {
        Normal,
        OneMoreEcho,   // 弱点ヒット → 追加行動
        Miss,
        Blocked,
        Reflected
    }
}
