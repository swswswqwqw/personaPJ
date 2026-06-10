using UnityEngine;

namespace Astra.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Astra/Data/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本情報")]
        public string enemyName;
        [TextArea(1, 3)] public string description;
        public bool isBoss;

        [Header("ステータス")]
        public int maxHP = 80;
        public int maxSP = 30;
        public int strength = 8;
        public int magic = 8;
        public int endurance = 8;
        public int agility = 8;
        public int luck = 5;

        [Header("属性耐性")]
        public AffinityType physicalAffinity = AffinityType.Normal;
        public AffinityType iraAffinity = AffinityType.Normal;
        public AffinityType trisAffinity = AffinityType.Normal;
        public AffinityType fulgoAffinity = AffinityType.Normal;
        public AffinityType ventoAffinity = AffinityType.Normal;
        public AffinityType luceAffinity = AffinityType.Normal;
        public AffinityType ombraAffinity = AffinityType.Normal;

        [Header("行動パターン")]
        public SkillData[] skills;
        [Range(0f, 1f)] public float skillUseRate = 0.5f;

        [Header("報酬")]
        public int expReward = 10;
        public int moneyReward = 100;

        [Header("ビジュアル")]
        public Sprite battleSprite;

        public AffinityType GetAffinity(ElementType element)
        {
            return element switch
            {
                ElementType.Physical => physicalAffinity,
                ElementType.Ira => iraAffinity,
                ElementType.Tris => trisAffinity,
                ElementType.Fulgo => fulgoAffinity,
                ElementType.Vento => ventoAffinity,
                ElementType.Luce => luceAffinity,
                ElementType.Ombra => ombraAffinity,
                ElementType.Almighty => AffinityType.Normal,
                _ => AffinityType.Normal
            };
        }
    }
}
