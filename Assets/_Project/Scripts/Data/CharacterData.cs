using System;
using UnityEngine;
using ArcadiaOfEchoes.Battle;

namespace ArcadiaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "ArcadiaOfEchoes/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string CharacterId;
        public string DisplayName;
        public string Arcana;
        [TextArea(2, 4)] public string Description;

        [Header("バトルステータス")]
        public int MaxHP = 100;
        public int MaxSP = 50;
        public int BaseAttack = 10;
        public int BaseDefense = 10;
        public int BaseSpeed = 10;

        [Header("属性相性")]
        public ElementAffinity[] Affinities;

        [Header("ビジュアル")]
        public Sprite Portrait;
        public Sprite BattleSprite;
        public RuntimeAnimatorController Animator;
    }

    [Serializable]
    public struct ElementAffinity
    {
        public ElementType Element;
        public AffinityType Affinity;
    }
}
