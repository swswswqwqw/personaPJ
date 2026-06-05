using System;
using System.Collections.Generic;
using AstralEchoes.Data;

namespace AstralEchoes.Data
{
    [Serializable]
    public class CharacterData
    {
        public string Id;
        public string NameJP;
        public string NameReading;
        public int Age;
        public Arcana Arcana;
        public bool IsPartyMember;
        public bool IsProtagonist;

        public CharacterStats BaseStats;
        public List<AttributeResistance> Resistances = new();
        public string DefaultEchoId;
    }

    [Serializable]
    public class CharacterStats
    {
        public int MaxHP = 100;
        public int MaxSP = 50;
        public int Strength = 10;
        public int Magic = 10;
        public int Endurance = 10;
        public int Agility = 10;
        public int Luck = 10;
        public int Level = 1;
    }

    [Serializable]
    public class AttributeResistance
    {
        public Attribute Attribute;
        public ResistanceLevel Level;
    }

    [Serializable]
    public class EchoData
    {
        public string Id;
        public string NameJP;
        public string NameEN;
        public Arcana Arcana;
        public int BaseLevel;
        public CharacterStats StatModifiers;
        public List<EchoSkill> Skills = new();
        public List<AttributeResistance> Resistances = new();
        public string Description;
    }

    [Serializable]
    public class EchoSkill
    {
        public string SkillId;
        public int LearnLevel;
    }

    [Serializable]
    public class SkillData
    {
        public string Id;
        public string NameJP;
        public string Description;
        public Attribute Attribute;
        public SkillType Type;
        public int BasePower;
        public int SPCost;
        public int HPCost;
        public TargetType Target;
        public float Accuracy = 1.0f;
        public float CritRate = 0.05f;
        public StatusAilment AilmentChance = StatusAilment.None;
        public float AilmentRate = 0f;
    }

    public enum SkillType
    {
        PhysicalAttack,
        MagicAttack,
        Heal,
        Support,
        Ailment
    }

    public enum TargetType
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }
}
