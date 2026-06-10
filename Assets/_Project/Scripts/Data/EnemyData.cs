using System.Collections.Generic;
using UnityEngine;

namespace AriaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "AriaOfEchoes/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本情報")]
        public string enemyName;
        [TextArea] public string description;
        public Sprite sprite;
        public EnemyCategory category;
        public int level = 1;

        [Header("ステータス")]
        public int hp = 100;
        public int sp = 30;
        public int strength = 10;
        public int magic = 10;
        public int endurance = 10;
        public int agility = 10;
        public int luck = 5;

        [Header("属性相性")]
        public List<ElementAffinity> affinities = new();

        [Header("行動パターン")]
        public List<EnemyAction> actions = new();

        [Header("ドロップ")]
        public int expReward = 10;
        public int moneyReward = 50;
        public List<ItemDrop> drops = new();

        public AffinityType GetAffinity(ElementType element)
        {
            var found = affinities.Find(a => a.element == element);
            return found?.affinity ?? AffinityType.Normal;
        }
    }

    public enum EnemyCategory
    {
        Normal,     // 通常の残響
        Mini,       // ミニボス（満月の残響）
        Boss,       // ボス（沈黙の主）
        Rare        // レア（雨の日限定）
    }

    [System.Serializable]
    public class EnemyAction
    {
        public SkillData skill;
        [Range(0, 100)] public int weight = 50;
        public EnemyActionCondition condition;
        public float hpThreshold = 1f;
    }

    public enum EnemyActionCondition
    {
        Always,
        HPBelow,
        HPAbove,
        AllyDown,
        FirstTurn
    }

    [System.Serializable]
    public class ItemDrop
    {
        public string itemName;
        [Range(0f, 1f)] public float dropRate = 0.1f;
    }
}
