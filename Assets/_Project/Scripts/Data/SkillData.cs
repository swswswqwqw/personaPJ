using UnityEngine;

namespace EchoesOfArcadia.Data
{
    public enum Element
    {
        Physical,
        Fire,
        Ice,
        Wind,
        Lightning,
        Light,
        Dark,
        Resonance,
        Almighty,
        Healing,
        Support
    }

    public enum SkillTarget
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "EchoesOfArcadia/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("基本情報")]
        public string skillName;
        public string furigana;
        [TextArea(1, 2)] public string description;

        [Header("属性・対象")]
        public Element element;
        public SkillTarget target;
        public int spCost;

        [Header("効果")]
        public int basePower;
        public float accuracy = 0.95f;
        public int criticalRate = 5;
        public int hitCount = 1;

        [Header("追加効果")]
        public StatusEffect statusEffect;
        public int statusChance;
        public int healPower;
        public float buffMultiplier;
    }

    public enum StatusEffect
    {
        None,
        Poison,
        Burn,
        Freeze,
        Shock,
        Dizzy,
        Sleep,
        Silence,
        AttackUp,
        DefenseUp,
        SpeedUp,
        AttackDown,
        DefenseDown,
        SpeedDown
    }
}
