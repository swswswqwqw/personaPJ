namespace Amane.Battle
{
    public enum Element
    {
        Slash,    // 斬
        Strike,   // 打
        Pierce,   // 貫
        Fire,     // 焔
        Ice,      // 氷
        Thunder,  // 雷
        Wind,     // 風
        Light,    // 光（赦し）
        Dark,     // 闇（後悔）
        Almighty  // 無（無効化不可）
    }

    public enum Affinity
    {
        Normal,
        Weak,     // 弱点 → 畳み掛け発生
        Resist,
        Null,     // 無効
        Drain,    // 吸収
        Repel     // 反射
    }
}
