using UnityEngine;

namespace EchoesOfArcadia.Data
{
    public enum Arcana
    {
        Fool,
        Magician,
        HighPriestess,
        Empress,
        Emperor,
        Hierophant,
        Lovers,
        Chariot,
        Justice,
        Hermit,
        Fortune,
        Strength,
        HangedMan,
        Death,
        Temperance,
        Devil,
        Tower,
        Star,
        Moon,
        Sun,
        Judgement,
        World
    }

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "EchoesOfArcadia/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string characterName;
        public string furigana;
        public int age;
        public Arcana arcana;
        [TextArea(2, 4)] public string description;

        [Header("戦闘")]
        public bool isPartyMember;
        public int baseHP = 100;
        public int baseSP = 50;
        public int baseStrength = 10;
        public int baseMagic = 10;
        public int baseEndurance = 10;
        public int baseAgility = 10;
        public int baseLuck = 10;

        [Header("属性耐性")]
        public ElementAffinity fireAffinity;
        public ElementAffinity iceAffinity;
        public ElementAffinity windAffinity;
        public ElementAffinity lightningAffinity;
        public ElementAffinity lightAffinity;
        public ElementAffinity darkAffinity;
        public ElementAffinity resonanceAffinity;
        public ElementAffinity physicalAffinity;

        [Header("共鳴（社会リンク）")]
        public int maxResonanceRank = 10;
        [TextArea(2, 4)] public string hiddenWound;
        [TextArea(2, 4)] public string growthArc;
    }

    public enum ElementAffinity
    {
        Normal,
        Weak,
        Resist,
        Null,
        Absorb,
        Repel
    }
}
