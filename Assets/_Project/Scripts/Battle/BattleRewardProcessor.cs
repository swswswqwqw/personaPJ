using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Battle
{
    public class BattleRewardProcessor : MonoBehaviour
    {
        public static BattleRewardProcessor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleStateChanged += OnBattleStateChanged;
        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleStateChanged -= OnBattleStateChanged;
        }

        private void OnBattleStateChanged(BattleState state)
        {
            if (state == BattleState.Victory)
                ProcessVictoryRewards();
        }

        private void ProcessVictoryRewards()
        {
            if (BattleManager.Instance == null) return;

            int totalExp = 0;
            int totalMoney = 0;
            var itemDrops = new List<string>();

            foreach (var enemy in BattleManager.Instance.EnemyUnits)
            {
                // BattleUnitからは直接EnemyDataを参照できないため、
                // 名前ベースで基本報酬を計算
                totalExp += CalculateBaseExp(enemy);
                totalMoney += CalculateBaseMoney(enemy);
            }

            // 絆ボーナス: 霧島葵（力）のランクに応じてEXPボーナス
            float expMultiplier = GetExpMultiplier();
            totalExp = Mathf.RoundToInt(totalExp * expMultiplier);

            // 神楽祐介（戦車）のランクに応じてアイテムドロップ率ボーナス
            float dropMultiplier = GetDropRateMultiplier();

            GameEventBus.Publish(new BattleRewardEvent(totalExp, totalMoney, itemDrops));

            // 勇気ステータスに少量加算（戦闘による成長）
            PlayerStats.Instance?.AddPoints(PersonalStat.Courage, 1);
        }

        private int CalculateBaseExp(BattleUnit enemy)
        {
            return (enemy.Strength + enemy.Magic + enemy.Endurance) * 2;
        }

        private int CalculateBaseMoney(BattleUnit enemy)
        {
            return (enemy.Strength + enemy.Magic) * 5;
        }

        private float GetExpMultiplier()
        {
            var socialManager = Social.SocialLinkManager.Instance;
            if (socialManager == null) return 1f;

            int strengthRank = socialManager.GetBondRank(Arcana.Strength);
            return 1f + strengthRank * 0.05f;
        }

        private float GetDropRateMultiplier()
        {
            var socialManager = Social.SocialLinkManager.Instance;
            if (socialManager == null) return 1f;

            int chariotRank = socialManager.GetBondRank(Arcana.Chariot);
            return 1f + chariotRank * 0.1f;
        }
    }

    public readonly struct BattleRewardEvent
    {
        public readonly int TotalExp;
        public readonly int TotalMoney;
        public readonly List<string> ItemDrops;

        public BattleRewardEvent(int exp, int money, List<string> items)
        {
            TotalExp = exp;
            TotalMoney = money;
            ItemDrops = items;
        }
    }
}
