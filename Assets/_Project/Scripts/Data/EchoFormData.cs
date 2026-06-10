using UnityEngine;

namespace EchoesOfArcadia.Data
{
    [CreateAssetMenu(fileName = "NewEchoForm", menuName = "EchoesOfArcadia/Echo Form Data")]
    public class EchoFormData : ScriptableObject
    {
        [Header("基本情報")]
        public string echoFormName;
        public string furigana;
        public Arcana arcana;
        public int level;
        [TextArea(2, 4)] public string description;

        [Header("ステータス")]
        public int strength;
        public int magic;
        public int endurance;
        public int agility;
        public int luck;

        [Header("属性耐性")]
        public ElementAffinity fireAffinity;
        public ElementAffinity iceAffinity;
        public ElementAffinity windAffinity;
        public ElementAffinity lightningAffinity;
        public ElementAffinity lightAffinity;
        public ElementAffinity darkAffinity;
        public ElementAffinity resonanceAffinity;
        public ElementAffinity physicalAffinity;

        [Header("スキル")]
        public SkillData[] initialSkills;
        public LearnableSkill[] learnableSkills;
    }

    [System.Serializable]
    public struct LearnableSkill
    {
        public int levelRequired;
        public SkillData skill;
    }
}
