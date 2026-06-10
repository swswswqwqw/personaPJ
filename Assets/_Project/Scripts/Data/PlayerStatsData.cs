using System;
using UnityEngine;

namespace ArcadiaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "ArcadiaOfEchoes/Player Stats Config")]
    public class PlayerStatsData : ScriptableObject
    {
        public SocialStatConfig Courage;
        public SocialStatConfig Wisdom;
        public SocialStatConfig Sensibility;
        public SocialStatConfig Guts;
        public SocialStatConfig Empathy;

        public SocialStatConfig GetConfig(SocialStatType type)
        {
            return type switch
            {
                SocialStatType.Courage => Courage,
                SocialStatType.Wisdom => Wisdom,
                SocialStatType.Sensibility => Sensibility,
                SocialStatType.Guts => Guts,
                SocialStatType.Empathy => Empathy,
                _ => null
            };
        }
    }

    [Serializable]
    public class SocialStatConfig
    {
        public string StatName;
        public string[] RankNames = new string[5]
        {
            "平凡", "堅実", "熟練", "達人", "極"
        };
        public int[] ThresholdsPerRank = new int[5]
        {
            0, 15, 30, 50, 80
        };
    }

    public enum SocialStatType
    {
        Courage,
        Wisdom,
        Sensibility,
        Guts,
        Empathy
    }
}
