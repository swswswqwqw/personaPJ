namespace AriaOfEchoes.Data
{
    public enum ElementType
    {
        Shout,    // 叫 — 怒り・抗議（赤）
        Lament,   // 嘆 — 悲しみ・喪失（青）
        Whisper,  // 囁 — 不安・恐怖（紫）
        Harmony,  // 諧 — 喜び・調和（緑）
        Thunder,  // 轟 — 衝撃・驚愕（黄）
        Silence,  // 寂 — 虚無・諦め（灰）
        Resonance,// 響 — 共鳴・万能（白）
        Physical, // 物理
        Heal      // 回復
    }

    public enum AffinityType
    {
        Normal,
        Weak,
        Resist,
        Null,
        Reflect,
        Absorb
    }
}
