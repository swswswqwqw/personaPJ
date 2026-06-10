using System;
using System.Collections.Generic;
using UnityEngine;

namespace AriaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewResonanceForm", menuName = "AriaOfEchoes/ResonanceFormData")]
    public class ResonanceFormData : ScriptableObject
    {
        [Header("基本情報")]
        public string formName;
        [TextArea] public string description;
        public ArcanaType arcana;
        public int level = 1;
        public Sprite icon;

        [Header("ステータス補正")]
        public int hpBonus;
        public int spBonus;
        public int strengthBonus;
        public int magicBonus;
        public int enduranceBonus;
        public int agilityBonus;
        public int luckBonus;

        [Header("属性相性")]
        public List<ElementAffinity> affinities = new();

        [Header("習得スキル")]
        public List<SkillLearnEntry> learnableSkills = new();

        public AffinityType GetAffinity(ElementType element)
        {
            var found = affinities.Find(a => a.element == element);
            return found?.affinity ?? AffinityType.Normal;
        }

        public List<SkillData> GetSkillsAtLevel(int currentLevel)
        {
            var skills = new List<SkillData>();
            foreach (var entry in learnableSkills)
            {
                if (entry.learnLevel <= currentLevel)
                    skills.Add(entry.skill);
            }
            return skills;
        }
    }

    [Serializable]
    public class SkillLearnEntry
    {
        public int learnLevel;
        public SkillData skill;
    }
}
