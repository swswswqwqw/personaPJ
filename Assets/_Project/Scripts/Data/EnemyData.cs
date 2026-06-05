using UnityEngine;
using ArcadiaOfEchoes.Battle;

namespace ArcadiaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "ArcadiaOfEchoes/Enemy Data")]
    public class EnemyData : CharacterData
    {
        [Header("エネミー情報")]
        public int ExpReward = 10;
        public int MoneyReward = 50;
        [TextArea(1, 2)] public string EnemyType;

        [Header("行動パターン")]
        public EnemyActionPattern[] ActionPatterns;
    }

    [System.Serializable]
    public struct EnemyActionPattern
    {
        public SkillData Skill;
        [Range(0f, 1f)] public float Weight;
        public float HPThresholdBelow;
    }
}
