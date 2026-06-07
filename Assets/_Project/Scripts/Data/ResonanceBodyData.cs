using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfArcadia.Data
{
    [CreateAssetMenu(fileName = "NewResonanceBody", menuName = "EchoesOfArcadia/Resonance Body Data")]
    public class ResonanceBodyData : ScriptableObject
    {
        [Header("基本情報")]
        public string bodyName;
        public Arcana arcana;
        public int baseLevel = 1;
        [TextArea(1, 3)] public string description;

        [Header("ステータス")]
        public int baseHP = 80;
        public int baseSP = 40;
        public int strength = 8;
        public int magic = 8;
        public int endurance = 8;
        public int agility = 8;
        public int luck = 8;

        [Header("属性耐性")]
        public AffinityEntry[] affinities;

        [Header("スキル習得")]
        public LearnableAbility[] learnableAbilities;

        [Header("合体素材")]
        public bool canBeUsedInFusion = true;
        public int fusionCost;
    }

    [System.Serializable]
    public struct LearnableAbility
    {
        public AbilityData ability;
        public int learnLevel;
    }

    [CreateAssetMenu(fileName = "NewFusionRecipe", menuName = "EchoesOfArcadia/Fusion Recipe")]
    public class FusionRecipe : ScriptableObject
    {
        public ResonanceBodyData material1;
        public ResonanceBodyData material2;
        public ResonanceBodyData result;
        public bool requiresSpecialCondition;
        [TextArea(1, 2)] public string conditionDescription;
    }
}
