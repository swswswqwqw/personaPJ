using System;
using UnityEngine;

namespace ArcadiaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "ArcadiaOfEchoes/Skill")]
    public class SkillData : ScriptableObject
    {
        [Header("基本情報")]
        public string SkillId;
        public string SkillName;
        [TextArea] public string Description;

        [Header("属性・タイプ")]
        public ElementType Element;
        public SkillType Type;
        public SkillTarget Target;

        [Header("コスト")]
        public int SPCost;
        public int HPCost;

        [Header("威力")]
        public int BasePower;
        public int HitCount = 1;
        public float CriticalRate;

        [Header("追加効果")]
        public StatusEffect AdditionalEffect;
        public float EffectChance;

        [Header("回復・バフ")]
        public int HealAmount;
        public float BuffMultiplier = 1f;
        public int BuffDuration;
    }

    public enum SkillType
    {
        Attack,
        Heal,
        Support,
        Ailment,
        Passive
    }

    public enum SkillTarget
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    public enum StatusEffect
    {
        None,
        Poison,
        Burn,
        Freeze,
        Shock,
        Sleep,
        Confuse,
        Fear,
        Rage,
        Despair,
        AttackUp,
        DefenseUp,
        SpeedUp,
        AttackDown,
        DefenseDown,
        SpeedDown
    }
}
