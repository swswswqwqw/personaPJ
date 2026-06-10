using UnityEngine;

namespace EchoesOfArcadia.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "EchoesOfArcadia/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本情報")]
        public string enemyName;
        public string furigana;
        public int level;
        public bool isBoss;
        [TextArea(1, 3)] public string description;

        [Header("ステータス")]
        public int hp = 100;
        public int sp = 30;
        public int strength = 10;
        public int magic = 10;
        public int endurance = 10;
        public int agility = 10;
        public int luck = 5;

        [Header("属性耐性")]
        public ElementAffinity fireAffinity;
        public ElementAffinity iceAffinity;
        public ElementAffinity windAffinity;
        public ElementAffinity lightningAffinity;
        public ElementAffinity lightAffinity;
        public ElementAffinity darkAffinity;
        public ElementAffinity resonanceAffinity;
        public ElementAffinity physicalAffinity;

        [Header("行動パターン")]
        public SkillData[] skills;
        public EnemyAIPattern aiPattern;

        [Header("ドロップ")]
        public int expReward;
        public int moneyReward;
        public ItemData dropItem;
        public float dropRate = 0.2f;
    }

    public enum EnemyAIPattern
    {
        Random,
        AggressivePhysical,
        AggressiveMagic,
        Healer,
        Support,
        ExploitWeakness,
        BossPhased
    }
}
