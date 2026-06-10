using UnityEngine;

namespace Astra.Data
{
    public enum SkillTarget
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    public enum SkillCategory
    {
        Attack,
        Heal,
        Support,
        Ailment
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "Astra/Data/SkillData")]
    public class SkillData : ScriptableObject
    {
        [Header("基本情報")]
        public string skillName;
        [TextArea(1, 3)] public string description;
        public SkillCategory category;
        public ElementType element;
        public SkillTarget target;

        [Header("コスト")]
        public int spCost;
        public int hpCost;

        [Header("効果")]
        public int power;
        public float accuracy = 0.95f;
        public int criticalRate = 5;
        public int hitCount = 1;

        [Header("演出")]
        public string animationKey;
        public string seKey;
        public float effectDuration = 1.0f;
    }
}
