using UnityEngine;

namespace EchoesOfArcadia.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "EchoesOfArcadia/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本情報")]
        public string enemyName;
        [TextArea(1, 3)] public string description;
        public int level;
        public bool isBoss;

        [Header("ステータス")]
        public int hp = 80;
        public int sp = 30;
        public int strength = 8;
        public int magic = 8;
        public int endurance = 8;
        public int agility = 8;
        public int luck = 8;

        [Header("属性耐性")]
        public AffinityEntry[] affinities;

        [Header("行動パターン")]
        public AbilityData[] abilities;

        [Header("報酬")]
        public int expReward = 10;
        public int moneyReward = 50;
        public ItemDrop[] itemDrops;
    }

    [System.Serializable]
    public struct ItemDrop
    {
        public string itemName;
        [Range(0f, 1f)] public float dropRate;
    }
}
