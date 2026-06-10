using System;
using System.Collections.Generic;

namespace Amane.Battle
{
    /// <summary>
    /// 経験値・レベルアップシステム。
    /// ペルソナ的な成長カーブ: 序盤は速く、中盤から緩やかに。
    /// </summary>
    public sealed class ExperienceSystem
    {
        // レベルごとの累積EXP閾値（レベル1=0, レベル2=30, ...）
        private static readonly int[] ExpTable = GenerateExpTable(99);

        private readonly Dictionary<string, CharacterExp> _characters = new();

        public event Action<string, int, int> OnLevelUp; // id, newLevel, oldLevel

        /// <summary>キャラクターを登録する</summary>
        public void Register(string id, int startLevel = 1)
        {
            if (_characters.ContainsKey(id)) return;
            var data = new CharacterExp
            {
                Level = startLevel,
                TotalExp = startLevel > 1 ? GetExpForLevel(startLevel) : 0
            };
            _characters[id] = data;
        }

        /// <summary>戦闘終了後にEXPを付与する</summary>
        public List<LevelUpResult> GiveExp(string id, int exp)
        {
            var results = new List<LevelUpResult>();
            if (!_characters.TryGetValue(id, out var data)) return results;

            data.TotalExp += exp;
            int oldLevel = data.Level;

            while (data.Level < 99 && data.TotalExp >= GetExpForLevel(data.Level + 1))
            {
                data.Level++;
                var growth = GetStatGrowth(data.Level);
                results.Add(new LevelUpResult
                {
                    NewLevel = data.Level,
                    HpGain = growth.hp,
                    SpGain = growth.sp,
                    AtkGain = growth.atk,
                    DefGain = growth.def,
                    MatGain = growth.matk,
                    MdfGain = growth.mdef,
                    AgiGain = growth.agi
                });
            }

            if (data.Level > oldLevel)
                OnLevelUp?.Invoke(id, data.Level, oldLevel);

            return results;
        }

        /// <summary>敵を全滅させた時のEXP計算</summary>
        public static int CalculateBattleExp(List<Combatant> enemies)
        {
            int total = 0;
            foreach (var enemy in enemies)
            {
                // 基本EXP = (HP + Attack + MagicAttack) / 3
                total += (enemy.MaxHp + enemy.Attack + enemy.MagicAttack) / 3;
            }
            return Math.Max(10, total);
        }

        public int GetLevel(string id) =>
            _characters.TryGetValue(id, out var d) ? d.Level : 1;

        public int GetTotalExp(string id) =>
            _characters.TryGetValue(id, out var d) ? d.TotalExp : 0;

        public int GetExpToNext(string id)
        {
            if (!_characters.TryGetValue(id, out var d)) return 0;
            if (d.Level >= 99) return 0;
            return GetExpForLevel(d.Level + 1) - d.TotalExp;
        }

        public float GetExpProgress(string id)
        {
            if (!_characters.TryGetValue(id, out var d)) return 0;
            if (d.Level >= 99) return 1f;
            int currentLevelExp = GetExpForLevel(d.Level);
            int nextLevelExp = GetExpForLevel(d.Level + 1);
            int range = nextLevelExp - currentLevelExp;
            if (range <= 0) return 1f;
            return (float)(d.TotalExp - currentLevelExp) / range;
        }

        // ---- 内部 ----

        private static int GetExpForLevel(int level)
        {
            if (level <= 1) return 0;
            if (level - 2 < ExpTable.Length) return ExpTable[level - 2];
            return ExpTable[^1] + (level - ExpTable.Length - 1) * 500;
        }

        private static int[] GenerateExpTable(int maxLevel)
        {
            // ペルソナ風の成長カーブ: 序盤速い、中盤緩やか、終盤きつい
            var table = new int[maxLevel - 1];
            int cumulative = 0;
            for (int lv = 2; lv <= maxLevel; lv++)
            {
                // 必要EXP = 20 + level^1.5 * 3
                int required = 20 + (int)(Math.Pow(lv, 1.5) * 3);
                cumulative += required;
                table[lv - 2] = cumulative;
            }
            return table;
        }

        private static (int hp, int sp, int atk, int def, int matk, int mdef, int agi)
            GetStatGrowth(int newLevel)
        {
            // レベルごとのステータス上昇量
            return (
                hp: 8 + newLevel / 5,     // HP: 8〜28
                sp: 3 + newLevel / 8,     // SP: 3〜15
                atk: 1 + (newLevel % 3 == 0 ? 1 : 0),  // ATK: 1〜2
                def: 1 + (newLevel % 4 == 0 ? 1 : 0),   // DEF: 1〜2
                matk: 1 + (newLevel % 3 == 1 ? 1 : 0),  // MATK: 1〜2
                mdef: 1 + (newLevel % 4 == 1 ? 1 : 0),  // MDEF: 1〜2
                agi: (newLevel % 5 == 0 ? 1 : 0)         // AGI: 0〜1
            );
        }

        private class CharacterExp
        {
            public int Level;
            public int TotalExp;
        }
    }

    public struct LevelUpResult
    {
        public int NewLevel;
        public int HpGain, SpGain, AtkGain, DefGain, MatGain, MdfGain, AgiGain;

        public override string ToString() =>
            $"Lv.{NewLevel} HP+{HpGain} SP+{SpGain} ATK+{AtkGain} DEF+{DefGain} " +
            $"MAT+{MatGain} MDF+{MdfGain} AGI+{AgiGain}";
    }
}
