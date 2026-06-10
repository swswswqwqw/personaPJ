using System;
using System.Collections.Generic;
using UnityEngine;
using ArcadiaOfEchoes.Data;

namespace ArcadiaOfEchoes.Battle
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "ArcadiaOfEchoes/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本情報")]
        public string EnemyId;
        public string EnemyName;
        public string NameReading;
        public EnemyType Type;
        [TextArea] public string Description;

        [Header("ステータス")]
        public CharacterStats BaseStats;
        public int Level = 1;
        public int ExpReward;
        public int MoneyReward;

        [Header("属性耐性")]
        public List<ElementAffinity> Affinities;

        [Header("スキル")]
        public List<SkillData> Skills;

        [Header("AI")]
        public EnemyAIPattern AIPattern;

        [Header("ドロップ")]
        public List<DropItem> Drops;
    }

    public enum EnemyType
    {
        Normal,
        MiniBoss,
        Boss,
        Rare
    }

    public enum EnemyAIPattern
    {
        Aggressive,
        Defensive,
        Balanced,
        Support,
        Berserk
    }

    [Serializable]
    public class DropItem
    {
        public string ItemId;
        public float DropRate;
    }
}
