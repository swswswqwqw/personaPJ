using UnityEngine;

namespace AriaOfEchoes.Data
{
    public enum SkillTarget
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "AriaOfEchoes/SkillData")]
    public class SkillData : ScriptableObject
    {
        [Header("基本情報")]
        public string skillName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("属性・タイプ")]
        public ElementType element;
        public SkillTarget target;

        [Header("コスト")]
        public int spCost;
        public int hpCost;

        [Header("威力")]
        public int basePower;
        public float criticalRate;
        public float accuracy = 0.95f;

        [Header("追加効果")]
        public StatusEffect statusEffect;
        public float statusEffectChance;
        public int hitCount = 1;

        public bool IsPhysical => element == ElementType.Physical;
        public bool IsMagical => element != ElementType.Physical
                              && element != ElementType.Heal;
        public bool IsHeal => element == ElementType.Heal;
    }

    public enum StatusEffect
    {
        None,
        Poison,
        Silence,  // 沈黙（スキル使用不可）
        Fear,     // 恐怖（行動不能確率）
        Rage,     // 激昂（物理のみ・攻撃UP防御DOWN）
        Despair,  // 絶望（SP減少・行動不能確率）
        Confusion,// 混乱（ランダム行動）
        Charm     // 魅了（敵を回復する可能性）
    }
}
