using UnityEngine;

namespace EchoesOfArcadia.UI
{
    public static class UIColors
    {
        public static readonly Color DeepIndigo = new(0.102f, 0.102f, 0.306f, 1f);    // #1A1A4E
        public static readonly Color Cyan = new(0f, 0.831f, 0.667f, 1f);               // #00D4AA
        public static readonly Color Amber = new(1f, 0.702f, 0.278f, 1f);              // #FFB347
        public static readonly Color CreamWhite = new(1f, 0.973f, 0.933f, 1f);         // #FFF8EE
        public static readonly Color MidnightBlue = new(0.051f, 0.051f, 0.169f, 1f);   // #0D0D2B
        public static readonly Color Crimson = new(0.863f, 0.078f, 0.235f, 1f);        // #DC143C
        public static readonly Color OffBlack = new(0.102f, 0.102f, 0.102f, 1f);       // #1A1A1A

        public static readonly Color BlazeRed = new(0.9f, 0.2f, 0.15f, 1f);
        public static readonly Color FrostBlue = new(0.2f, 0.5f, 0.95f, 1f);
        public static readonly Color GaleGreen = new(0.2f, 0.85f, 0.4f, 1f);
        public static readonly Color VoltYellow = new(0.95f, 0.85f, 0.15f, 1f);
        public static readonly Color NovaWhite = new(1f, 1f, 0.9f, 1f);
        public static readonly Color VoidPurple = new(0.5f, 0.15f, 0.7f, 1f);

        public static Color GetElementColor(Data.ElementType element)
        {
            return element switch
            {
                Data.ElementType.Blaze => BlazeRed,
                Data.ElementType.Frost => FrostBlue,
                Data.ElementType.Gale => GaleGreen,
                Data.ElementType.Volt => VoltYellow,
                Data.ElementType.Nova => NovaWhite,
                Data.ElementType.Void => VoidPurple,
                _ => Color.white
            };
        }
    }
}
