using UnityEngine;

namespace Astra.Data
{
    public enum ElementType
    {
        Physical,   // 物理
        Ira,        // 炎（怒り）
        Tris,       // 氷（悲しみ）
        Fulgo,      // 雷（恐怖）
        Vento,      // 風（焦り）
        Luce,       // 光（希望）
        Ombra,      // 闇（絶望）
        Almighty    // 万能
    }

    public enum AffinityType
    {
        Normal,
        Weak,
        Resist,
        Null,
        Absorb,
        Repel
    }

    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Astra/Data/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string characterName;
        public string arcanaName;
        public int age;
        [TextArea(2, 4)] public string description;

        [Header("ステータス")]
        public int maxHP = 100;
        public int maxSP = 50;
        public int strength = 10;
        public int magic = 10;
        public int endurance = 10;
        public int agility = 10;
        public int luck = 10;

        [Header("属性耐性")]
        public AffinityType physicalAffinity = AffinityType.Normal;
        public AffinityType iraAffinity = AffinityType.Normal;
        public AffinityType trisAffinity = AffinityType.Normal;
        public AffinityType fulgoAffinity = AffinityType.Normal;
        public AffinityType ventoAffinity = AffinityType.Normal;
        public AffinityType luceAffinity = AffinityType.Normal;
        public AffinityType ombraAffinity = AffinityType.Normal;

        [Header("共鳴リンク")]
        public int resonanceLinkRank = 0;
        public int maxResonanceLinkRank = 10;

        [Header("ビジュアル")]
        public Sprite portrait;
        public Sprite battleSprite;
        public Sprite cutInSprite;

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
