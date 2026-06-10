using UnityEngine;

namespace ArcanaOfHollows.Data
{
    public enum SkillType
    {
        Attack,
        Heal,
        Support,
        Ailment
    }

    public enum TargetType
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "ArcanaOfHollows/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Basic")]
        public string skillName;
        public string skillNameJP;
        [TextArea(1, 2)] public string description;
        public Sprite icon;

        [Header("Type")]
        public SkillType skillType = SkillType.Attack;
        public Element element = Element.Physical;
        public TargetType targetType = TargetType.SingleEnemy;

        [Header("Power")]
        public int basePower = 50;
        public int spCost = 5;
        public float accuracy = 0.95f;
        public int hitCount = 1;

        [Header("Flags")]
        public bool isPhysical;

        public bool IsPhysical => isPhysical || element == Element.Physical;
        public int BasePower => basePower;
        public Element Element => element;
    }
}
