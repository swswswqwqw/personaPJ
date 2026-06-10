using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadiaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewResonanceForm", menuName = "ArcadiaOfEchoes/Resonance Form")]
    public class ResonanceFormData : ScriptableObject
    {
        [Header("基本情報")]
        public string FormId;
        public string FormName;
        public string NameReading;
        public ArcanaType Arcana;
        public int BaseLevel = 1;
        [TextArea] public string Lore;

        [Header("ステータス")]
        public CharacterStats BaseStats;

        [Header("属性耐性")]
        public List<ElementAffinity> Affinities;

        [Header("スキル")]
        public List<LearnableSkill> Skills;

        [Header("合体")]
        public bool CanFuse = true;
        public int FusionCost;
    }

    [Serializable]
    public class ElementAffinity
    {
        public ElementType Element;
        public AffinityType Affinity;
    }

    [Serializable]
    public class LearnableSkill
    {
        public SkillData Skill;
        public int LearnLevel;
    }

    public enum ElementType
    {
        Blaze,   // 炎 — 怒り
        Frost,   // 氷 — 悲嘆
        Volt,    // 雷 — 衝動
        Gale,    // 風 — 自由
        Lux,     // 光 — 希望
        Nox,     // 闇 — 秘密
        Psyche,  // 念 — 執着
        Strike,  // 物理
        Almighty // 万能（耐性無視）
    }

    public enum AffinityType
    {
        Weak,     // 弱点（ダメージ1.5倍 + 1More）
        Normal,   // 通常
        Resist,   // 耐性（ダメージ0.5倍）
        Null,     // 無効
        Absorb,   // 吸収（HP回復）
        Reflect   // 反射
    }

    public enum BattleRole
    {
        Attacker,
        Supporter,
        Tank,
        Speedster,
        AllRounder
    }
}
