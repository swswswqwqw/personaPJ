using UnityEngine;

namespace AriaOfEchoes.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "AriaOfEchoes/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("ゲーム基本設定")]
        public string gameTitle = "残響のアリア";
        public string gameVersion = "0.1.0";

        [Header("時間設定")]
        public int startYear = 202;
        public int startMonth = 4;
        public int startDay = 1;

        [Header("戦闘設定")]
        public int maxPartySize = 4;
        public float weaknessMultiplier = 1.5f;
        public float resistanceMultiplier = 0.5f;

        [Header("絆設定")]
        public int maxBondRank = 10;
        public int baseBondPointsPerRank = 100;
    }
}
