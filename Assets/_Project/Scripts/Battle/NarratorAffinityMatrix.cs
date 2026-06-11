namespace Amane.Battle
{
    // 語り手間の属性相性マトリクス（DESIGN.md 9-1: デュアルナレーター）。
    // 同属性: x1.2 シナジー（声が共鳴する）
    // 対立属性: x0.7 干渉（声が相殺し合う）
    // その他: x1.0 中立
    // テーマ根拠: 相反する感情の同時保持が「力」にも「歪み」にもなる両義性。
    public static class NarratorAffinityMatrix
    {
        public static float GetDualBonus(Element a, Element b)
        {
            if (a == b) return 1.2f;
            if (IsOpposed(a, b)) return 0.7f;
            return 1.0f;
        }

        // 対立ペアの定義（光/闇・焔/氷・雷/風）
        private static bool IsOpposed(Element a, Element b)
        {
            return (a == Element.Light   && b == Element.Dark)    ||
                   (a == Element.Dark    && b == Element.Light)   ||
                   (a == Element.Fire    && b == Element.Ice)     ||
                   (a == Element.Ice     && b == Element.Fire)    ||
                   (a == Element.Thunder && b == Element.Wind)    ||
                   (a == Element.Wind    && b == Element.Thunder);
        }
    }
}
