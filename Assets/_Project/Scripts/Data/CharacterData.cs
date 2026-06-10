using UnityEngine;

namespace ArcanaOfHollows.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "ArcanaOfHollows/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Basic Info")]
        public string characterName;
        public string surname;
        public int age;
        [TextArea(2, 4)] public string description;
        public Sprite portrait;
        public Sprite battleSprite;
        public Arcana arcana;

        [Header("Base Stats")]
        public int maxHP = 100;
        public int maxSP = 50;
        public int strength = 10;
        public int magic = 10;
        public int endurance = 10;
        public int agility = 10;
        public int luck = 10;

        [Header("Element Affinities (Physical, Fire, Ice, Wind, Electric, Psychic, Nuclear, Almighty)")]
        public ElementAffinity[] elementAffinities = new ElementAffinity[8];

        [Header("Skills")]
        public SkillData[] skills;

        [Header("Echo (Persona equivalent)")]
        public EchoData initialEcho;
        public EchoData evolvedEcho;
    }

    public enum Arcana
    {
        Fool,       // 愚者
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
