using AriaOfBacklight.Data;

namespace AriaOfBacklight.Battle
{
    public enum Effectiveness
    {
        Normal,
        Weak,
        Resist,
        Null,
        Absorb
    }

    public static class ElementChart
    {
        // 7 elements (excluding Physical and None): Homura, Hyoketsu, Raimei, Hayate, Kouki, Shinen, Kyomei
        // Row = attack element, Column = target weakness
        private static readonly Effectiveness[,] Chart = new Effectiveness[9, 9];

        static ElementChart()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    Chart[i, j] = Effectiveness.Normal;

            Set(ElementType.Homura, ElementType.Hyoketsu, Effectiveness.Weak);
            Set(ElementType.Hyoketsu, ElementType.Hayate, Effectiveness.Weak);
            Set(ElementType.Raimei, ElementType.Homura, Effectiveness.Weak);
            Set(ElementType.Hayate, ElementType.Raimei, Effectiveness.Weak);

            Set(ElementType.Kouki, ElementType.Shinen, Effectiveness.Weak);
            Set(ElementType.Shinen, ElementType.Kouki, Effectiveness.Weak);

            Set(ElementType.Homura, ElementType.Homura, Effectiveness.Resist);
            Set(ElementType.Hyoketsu, ElementType.Hyoketsu, Effectiveness.Resist);
            Set(ElementType.Raimei, ElementType.Raimei, Effectiveness.Resist);
            Set(ElementType.Hayate, ElementType.Hayate, Effectiveness.Resist);
            Set(ElementType.Kouki, ElementType.Kouki, Effectiveness.Resist);
            Set(ElementType.Shinen, ElementType.Shinen, Effectiveness.Resist);

            Set(ElementType.Kyomei, ElementType.None, Effectiveness.Weak);
        }

        private static void Set(ElementType attack, ElementType targetWeakness, Effectiveness eff)
        {
            Chart[(int)attack, (int)targetWeakness] = eff;
        }

        public static Effectiveness GetEffectiveness(ElementType attackElement, ElementType targetWeakness)
        {
            if (attackElement == targetWeakness && attackElement != ElementType.Physical && attackElement != ElementType.None)
                return Effectiveness.Weak;

            return Chart[(int)attackElement, (int)targetWeakness];
        }
    }
}
