using UnityEngine;

namespace ArcanaOfHollows.Data
{
    [CreateAssetMenu(fileName = "NewEcho", menuName = "ArcanaOfHollows/Echo Data")]
    public class EchoData : ScriptableObject
    {
        [Header("Basic Info")]
        public string echoName;
        public string echoNameJP;
        [TextArea(2, 4)] public string mythology;
        public Sprite portrait;
        public Arcana arcana;
        public int level = 1;

        [Header("Stats")]
        public int strength = 10;
        public int magic = 10;
        public int endurance = 10;
        public int agility = 10;
        public int luck = 10;

        [Header("Element Affinities")]
        public ElementAffinity[] elementAffinities = new ElementAffinity[8];

        [Header("Skills (learned at specified levels)")]
        public LearnableSkill[] learnableSkills;

        [Header("Fusion")]
        public bool canBeFused = true;
        public int fusionCost = 0;
    }

    [System.Serializable]
    public class LearnableSkill
    {
        public SkillData skill;
        public int levelRequired;
    }
}
