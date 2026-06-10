namespace Amane.Battle
{
    public enum HitType
    {
        Normal,
        Weak,       // 弱点ヒット → 畳み掛け（One More）
        Critical,   // クリティカル → 畳み掛け
        Resist,
        Null,
        Drain,
        Repel,
        Miss
    }

    public readonly struct HitResult
    {
        public readonly Combatant Target;
        public readonly int Damage;
        public readonly HitType Type;
        public readonly bool CausedDown;

        public HitResult(Combatant target, int damage, HitType type, bool causedDown)
        {
            Target = target;
            Damage = damage;
            Type = type;
            CausedDown = causedDown;
        }

        public bool TriggersOneMore => Type == HitType.Weak || Type == HitType.Critical;
    }
}
