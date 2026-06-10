using UnityEngine;

namespace ArcanaOfHollows.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "ArcanaOfHollows/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName;
        [TextArea(1, 2)] public string description;
        public Sprite sprite;

        [Header("Stats")]
        public int maxHP = 80;
        public int maxSP = 30;
        public int strength = 8;
        public int magic = 8;
        public int endurance = 8;
        public int agility = 8;
        public int luck = 5;
        public int expReward = 20;
        public int goldReward = 50;

        [Header("Element Affinities")]
        public ElementAffinity[] elementAffinities = new ElementAffinity[8];

        [Header("Skills")]
        public SkillData[] skills;

        [Header("AI Behavior")]
        public EnemyAIType aiType = EnemyAIType.Random;
        public float weaknessTargetChance = 0.6f;
    }

    public enum EnemyAIType
    {
        Random,
        Aggressive,
        Defensive,
        Support,
        Boss
    }
}
