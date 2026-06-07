using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfArcadia.Data
{
    public enum Arcana
    {
        Fool,        // 0 - 愚者（主人公）
        Magician,    // I - 魔術師
        Priestess,   // II - 女教皇（詩織）
        Empress,     // III - 女帝
        Emperor,     // IV - 皇帝（凱）
        Hierophant,  // V - 法王
        Lovers,      // VI - 恋愛
        Chariot,     // VII - 戦車（祐介）
        Justice,     // VIII - 正義（陽菜）
        Hermit,      // IX - 隠者（源蔵）
        Fortune,     // X - 運命（龍一）
        Strength,    // XI - 力（葵）
        HangedMan,   // XII - 吊された男（みこと）
        Death,       // XIII - 死神（暁）
        Temperance,  // XIV - 節制
        Devil,       // XV - 悪魔
        Tower,       // XVI - 塔
        Star,        // XVII - 星
        Moon,        // XVIII - 月（沙耶）
        Sun,         // XIX - 太陽（花）
        Judgement,   // XX - 審判
        World        // XXI - 世界
    }

    public enum ElementType
    {
        Blaze,   // 烈火
        Frost,   // 凍結
        Gale,    // 疾風
        Volt,    // 轟雷
        Nova,    // 照光
        Void,    // 暗影
        Strike,  // 物理
        Heal     // 回復
    }

    public enum Affinity
    {
        Weak,
        Normal,
        Resist,
        Null,
        Absorb,
        Reflect
    }

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "EchoesOfArcadia/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string characterName;
        public string familyName;
        public int age;
        public Arcana arcana;
        [TextArea(2, 4)] public string shortBio;

        [Header("ステータス")]
        public int baseHP = 100;
        public int baseSP = 50;
        public int baseStrength = 10;
        public int baseMagic = 10;
        public int baseEndurance = 10;
        public int baseAgility = 10;
        public int baseLuck = 10;

        [Header("属性耐性")]
        public AffinityEntry[] affinities;

        [Header("共鳴体")]
        public string resonanceBodyName;
        public string evolvedResonanceBodyName;
        public int evolutionBondRank = 10;

        [Header("絆の調べ")]
        public bool isPartyMember;
        public int maxBondRank = 10;
        [TextArea(3, 6)] public string bondDescription;
    }

    [Serializable]
    public struct AffinityEntry
    {
        public ElementType element;
        public Affinity affinity;
    }
}
