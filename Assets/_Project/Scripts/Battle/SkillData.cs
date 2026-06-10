using UnityEngine;
using AriaOfBacklight.Data;

namespace AriaOfBacklight.Battle
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "AriaOfBacklight/SkillData")]
    public class SkillData : ScriptableObject
    {
        public string skillName;
        public string description;
        public ElementType element;
        public SkillTarget target;
        public int power;
        public int spCost;
        public int accuracy = 95;
        public bool isHealing;

        [Header("演出")]
        public string animationKey;
        public Color effectColor = Color.white;
    }

    public enum SkillTarget
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }
}
