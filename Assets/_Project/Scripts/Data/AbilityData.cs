using UnityEngine;
using Amane.Battle;

namespace Amane.Data
{
    [CreateAssetMenu(fileName = "NewAbility", menuName = "Amane/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [Header("基本情報")]
        public string abilityId;
        public string displayName;

        [Header("戦闘パラメータ")]
        public Element element;
        public int basePower;
        public int spCost;
        public TargetType target = TargetType.SingleEnemy;
        public bool isPhysical;
        [Range(0f, 1f)]
        public float critRate = 0.05f;

        [Header("テーマ的意味（世界観メモ）")]
        [TextArea(1, 2)]
        public string thematicNote;

        public Skill ToSkill() =>
            new Skill(abilityId, displayName, element, basePower, spCost, target, isPhysical, critRate);
    }
}
