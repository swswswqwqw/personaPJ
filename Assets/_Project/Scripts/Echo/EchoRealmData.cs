using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Data;
using EchoesOfArcadia.TimeSystem;

namespace EchoesOfArcadia.Echo
{
    [CreateAssetMenu(fileName = "NewEchoRealm", menuName = "EchoesOfArcadia/Echo Realm Dungeon")]
    public class EchoRealmData : ScriptableObject
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
        public GameDate deadline;
        public int warningDaysBeforeDeadline = 3;

        [Header("フロア構成")]
        public int totalFloors = 5;
        public EchoFloorData[] floors;

        [Header("ボス")]
        public EnemyData bossEnemy;
        [TextArea(2, 4)] public string bossThematicMeaning;

        [Header("ビジュアル")]
        public Color dominantColor = Color.blue;
        public string environmentTheme;
    }

    [Serializable]
    public class EchoFloorData
    {
        public int floorNumber;
        public string floorName;
        public EnemyData[] possibleEnemies;
        public int minEnemyLevel;
        public int maxEnemyLevel;
        public bool hasMiniBoss;
        public EnemyData miniBoss;
    }
}
