namespace Amane.Battle
{
    public enum TargetType
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    public sealed class Skill
    {
        public string Id { get; }
        public string DisplayName { get; }
        public Element Element { get; }
        public int BasePower { get; }
        public int SpCost { get; }
        public TargetType Target { get; }
        public bool IsPhysical { get; }
        public float CritRate { get; }

        public Skill(string id, string displayName, Element element, int basePower,
                     int spCost, TargetType target, bool isPhysical = false, float critRate = 0.05f)
        {
            Id = id;
            DisplayName = displayName;
            Element = element;
            BasePower = basePower;
            SpCost = spCost;
            Target = target;
            IsPhysical = isPhysical;
            CritRate = critRate;
        }

        public static Skill MeleeAttack => new("melee", "通常攻撃", Element.Strike, 30, 0,
            TargetType.SingleEnemy, isPhysical: true, critRate: 0.08f);
    }
}
