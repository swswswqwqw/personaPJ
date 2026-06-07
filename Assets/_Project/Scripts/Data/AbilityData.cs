using UnityEngine;

namespace EchoesOfArcadia.Data
{
    public enum TargetType
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "EchoesOfArcadia/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [Header("基本情報")]
        public string abilityName;
        [TextArea(1, 3)] public string description;
        public ElementType element;
        public TargetType targetType;

        [Header("コスト")]
        public int spCost;
        public int hpCost;

        [Header("効果")]
        public int basePower;
        public float accuracy = 0.95f;
        public int criticalRate = 5;
        public int hitCount = 1;

        [Header("追加効果")]
        public StatusEffect statusEffect = StatusEffect.None;
        public int statusChance;

        [Header("演出")]
        public float animationDuration = 1.0f;
        public string animationTrigger;
    }

    public enum StatusEffect
    {
        None,
        Burn,
        Freeze,
        Shock,
        Dizzy,
        Sleep,
        Forget,
        Silence,
        Fear,
        Despair,
        Rage
    }
}
