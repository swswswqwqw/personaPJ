using System;
using System.Collections.Generic;
using UnityEngine;

namespace AriaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "AriaOfEchoes/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string characterName;
        public string nameReading;
        public int age;
        [TextArea] public string description;
        public Sprite portrait;
        public Sprite battleSprite;

        [Header("アルカナ・絆")]
        public ArcanaType arcana;
        public int maxBondRank = 10;

        [Header("基本ステータス")]
        public int baseHP = 100;
        public int baseSP = 50;
        public int baseStrength = 10;
        public int baseMagic = 10;
        public int baseEndurance = 10;
        public int baseAgility = 10;
        public int baseLuck = 10;

        [Header("属性相性")]
        public List<ElementAffinity> affinities = new();

        [Header("初期残響体")]
        public ResonanceFormData initialResonanceForm;

        public AffinityType GetAffinity(ElementType element)
        {
            var found = affinities.Find(a => a.element == element);
            return found?.affinity ?? AffinityType.Normal;
        }
    }

    [Serializable]
    public class ElementAffinity
    {
        public ElementType element;
        public AffinityType affinity;
    }

    public enum ArcanaType
    {
        Fool,       // 愚者（主人公）
        Magician,   // 魔術師
        Priestess,  // 女教皇
        Empress,    // 女帝
        Emperor,    // 皇帝
        Hierophant, // 法王
        Lovers,     // 恋愛
        Chariot,    // 戦車
        Justice,    // 正義
        Hermit,     // 隠者
        Fortune,    // 運命
        Strength,   // 剛毅
        HangedMan,  // 刑死者
        Death,      // 死神
        Temperance, // 節制
        Devil,      // 悪魔
        Tower,      // 塔
        Star,       // 星
        Moon,       // 月
        Sun,        // 太陽
        Judgement,  // 審判
        World       // 世界
    }
}
