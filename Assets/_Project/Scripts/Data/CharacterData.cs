using UnityEngine;

namespace AriaOfBacklight.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "AriaOfBacklight/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string characterName;
        public string furigana;
        public int age;
        public Sprite portrait;
        public Sprite battleSprite;

        [Header("アルカナ・絆")]
        public ArcanaType arcana;
        public int maxBondRank = 10;

        [Header("戦闘パラメータ")]
        public int baseHP = 100;
        public int baseSP = 50;
        public int baseAttack = 10;
        public int baseDefense = 10;
        public int baseSpeed = 10;
        public ElementType primaryElement;
        public ElementType weakness;

        [Header("共鳴体")]
        public string resonanceBodyName;
        public string resonanceBodyDescription;
    }

    public enum ArcanaType
    {
        Fool,       // 0. 愚者
        Magician,   // I. 魔術師
        Priestess,  // II. 女教皇
        Empress,    // III. 女帝
        Emperor,    // IV. 皇帝
        Hierophant, // V. 法王
        Lovers,     // VI. 恋人
        Chariot,    // VII. 戦車
        Justice,    // VIII. 正義
        Hermit,     // IX. 隠者
        Fortune,    // X. 運命
        Strength,   // XI. 力
        HangedMan,  // XII. 刑死者
        Death,      // XIII. 死神
        Temperance, // XIV. 節制
        Devil,      // XV. 悪魔
        Tower,      // XVI. 塔
        Star,       // XVII. 星
        Moon,       // XVIII. 月
        Sun,        // XIX. 太陽
        Judgement,  // XX. 審判
        World       // XXI. 世界
    }

    public enum ElementType
    {
        Homura,   // 焔 — 怒り・情熱
        Hyoketsu, // 氷結 — 孤独・拒絶
        Raimei,   // 雷鳴 — 衝動・焦燥
        Hayate,   // 疾風 — 逃避・自由
        Kouki,    // 光輝 — 希望・理想
        Shinen,   // 深淵 — 絶望・諦め
        Kyomei,   // 共鳴 — 繋がり・共感
        Physical, // 物理
        None      // 無属性
    }
}
