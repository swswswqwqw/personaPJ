using System;
using System.Collections.Generic;
using EchoesOfArcadia.Data;
using EchoesOfArcadia.TimeSystem;

namespace EchoesOfArcadia.Core
{
    [Serializable]
    public class SaveData
    {
        public string saveId;
        public DateTime realWorldSaveTime;

        // 時間
        public GameDate currentDate;
        public TimeOfDay currentTimeOfDay;

        // プレイヤー
        public string playerName;
        public int playerLevel;
        public int currentHP;
        public int currentSP;
        public int money;

        // ステータス
        public int[] personalStats = new int[5];

        // パーティ
        public List<PartyMemberSave> partyMembers = new();

        // 絆の調べ
        public List<BondSave> bonds = new();

        // 共鳴体（所持リスト）
        public List<ResonanceBodySave> resonanceBodies = new();

        // フラグ
        public HashSet<string> storyFlags = new();
        public HashSet<string> completedEvents = new();

        // プレイ時間
        public float totalPlayTimeSeconds;
    }

    [Serializable]
    public class PartyMemberSave
    {
        public string characterId;
        public int level;
        public int currentHP;
        public int currentSP;
        public int exp;
        public bool isActive;
    }

    [Serializable]
    public class BondSave
    {
        public Arcana arcana;
        public int rank;
        public int points;
        public bool isMaxed;
    }

    [Serializable]
    public class ResonanceBodySave
    {
        public string resonanceBodyId;
        public int level;
        public int exp;
        public List<string> learnedAbilityIds = new();
    }
}
