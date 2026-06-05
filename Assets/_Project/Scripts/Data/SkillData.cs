using UnityEngine;
using ArcadiaOfEchoes.Battle;

namespace ArcadiaOfEchoes.Data
{
    public enum SkillTarget
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    [CreateAssetMenu(fileName = "NewSkill", menuName = "ArcadiaOfEchoes/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("基本情報")]
        public string SkillId;
        public string DisplayName;
        [TextArea(1, 3)] public string Description;

        [Header("性能")]
        public ElementType Element;
        public SkillTarget Target;
        public int Power = 50;
        public int SPCost = 5;
        public float CriticalRate = 0.05f;

        [Header("演出")]
        public string AnimationTrigger;
        public AudioClip SoundEffect;
    }
}
