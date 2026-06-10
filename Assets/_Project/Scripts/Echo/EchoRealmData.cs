using System;
using UnityEngine;
using Amane.Data;

namespace Amane.Echo
{
    [CreateAssetMenu(fileName = "NewMigenkai", menuName = "Amane/Migenkai Dungeon")]
    public class MigenkaiData : ScriptableObject
    {
        [Header("基本情報")]
        public string dungeonName;
        [TextArea(2, 4)] public string description;
        public int chapterNumber;

        [Header("被害者情報")]
        public string victimName;
        [TextArea(1, 3)] public string victimBackground;
        public string suppressedEmotion;

        [Header("デッドライン")]
        public int deadlineMonth;
        public int deadlineDay;
        public int warningDaysBeforeDeadline = 3;

        [Header("フロア構成")]
        public int totalFloors = 5;
        public MigenkaiFloorData[] floors;

        [Header("ボス")]
        public string bossName;
        [TextArea(2, 4)] public string bossThematicMeaning;

        [Header("ビジュアル")]
        public Color dominantColor = new(0.106f, 0.165f, 0.29f, 1f); // 群青 #1B2A4A
        public string environmentTheme;
    }

    [Serializable]
    public class MigenkaiFloorData
    {
        public int floorNumber;
        public string floorName;
        public int minEnemyLevel;
        public int maxEnemyLevel;
        public bool hasMiniBoss;
        public string miniBossName;
    }
}
