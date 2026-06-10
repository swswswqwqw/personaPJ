using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadiaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "ArcadiaOfEchoes/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string CharacterId;
        public string CharacterName;
        public string NameReading;
        public int Age;
        public string SchoolYear;
        [TextArea] public string AppearanceDescription;

        [Header("ストーリー")]
        public string PublicFace;
        [TextArea] public string HiddenWound;
        [TextArea] public string UnspokenWords;

        [Header("戦闘")]
        public bool IsPartyMember;
        public ElementType PrimaryElement;
        public BattleRole Role;
        public CharacterStats BaseStats;

        [Header("共鳴")]
        public ArcanaType Arcana;
        public int MaxResonanceRank = 10;
        public List<ResonanceReward> ResonanceRewards;

        [Header("台詞")]
        public DialogueLine AngerLine;
        public DialogueLine WoundLine;
        public DialogueLine GrowthLine;
        public DialogueLine BondLine;
    }

    [Serializable]
    public class CharacterStats
    {
        public int HP = 100;
        public int SP = 50;
        public int Strength = 10;
        public int Magic = 10;
        public int Endurance = 10;
        public int Agility = 10;
        public int Luck = 10;
    }

    [Serializable]
    public class ResonanceReward
    {
        public int RequiredRank;
        public string RewardDescription;
        public ResonanceRewardType Type;
        public string UnlockId;
    }

    [Serializable]
    public class DialogueLine
    {
        [TextArea] public string JapaneseText;
        public string SpeakerEmotion;
    }

    public enum ResonanceRewardType
    {
        Skill,
        StatBonus,
        BattleAbility,
        FieldAbility,
        UniqueSkill
    }

    public enum ArcanaType
    {
        Fool,
        Magician,
        Priestess,
        Empress,
        Emperor,
        Hierophant,
        Lovers,
        Chariot,
        Justice,
        Hermit,
        Fortune,
        Strength,
        HangedMan,
        Death,
        Temperance,
        Devil,
        Tower,
        Star,
        Moon,
        Sun,
        Judgement,
        World
    }
}
