using UnityEngine;
using System.Collections.Generic;
using Amane.Battle;

namespace Amane.Data
{
    /// <summary>
    /// 未言界の澱（敵）データ定義。
    /// ScriptableObjectとして Assets/ScriptableObjects/Enemies/ 以下に配置する。
    /// ToCombatant() で戦闘用 Combatant に変換する。
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Amane/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("基本情報")]
        public string enemyId;
        public string displayName;

        [Header("基礎ステータス")]
        public int maxHp = 100;
        public int maxSp = 30;
        public int attack = 16;
        public int defense = 14;
        public int magicAttack = 18;
        public int magicDefense = 14;
        public int agility = 12;

        [Header("属性耐性")]
        public Element[] weaknesses;
        public Element[] resistances;
        public Element[] nullifies;

        [Header("スキルセット")]
        public AbilityData[] abilities;

        [Header("戦闘報酬")]
        public int baseExp = 50;
        public int gold = 30;

        [Header("未言界テーマ設定")]
        [TextArea(1, 3)]
        public string thematicMeaning;
        public string suppressedEmotion;

        public Combatant ToCombatant()
        {
            var affinities = AffinityTable.Build(weaknesses, resistances, nullifies);
            var skills = new List<Skill>();
            if (abilities != null)
                foreach (var a in abilities)
                    if (a != null) skills.Add(a.ToSkill());
            if (skills.Count == 0)
                skills.Add(Skill.MeleeAttack);
            return new Combatant(enemyId, displayName, false,
                maxHp, maxSp, attack, defense, magicAttack, magicDefense, agility,
                affinities, skills);
        }
    }
}
