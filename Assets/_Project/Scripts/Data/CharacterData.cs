using UnityEngine;
using System.Collections.Generic;
using Amane.Battle;

namespace Amane.Data
{
    /// <summary>
    /// パーティメンバー1人分のデータ定義。
    /// ScriptableObjectとして Assets/ScriptableObjects/Characters/ 以下に配置する。
    /// ToCombatant() で戦闘用 Combatant に変換する。
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Amane/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public string characterId;
        public string displayName;
        public bool isPlayer = true;

        [Header("基礎ステータス")]
        public int maxHp = 150;
        public int maxSp = 80;
        public int attack = 20;
        public int defense = 18;
        public int magicAttack = 22;
        public int magicDefense = 20;
        public int agility = 18;

        [Header("属性耐性")]
        public Element[] weaknesses;
        public Element[] resistances;
        public Element[] nullifies;
        public Element[] drains;

        [Header("スキルセット")]
        public AbilityData[] abilities;

        [Header("キャラクター設定")]
        [TextArea(1, 3)]
        public string characterNote;

        public Combatant ToCombatant()
        {
            var affinities = AffinityTable.Build(weaknesses, resistances, nullifies, drains);
            var skills = new List<Skill>();
            if (abilities != null)
                foreach (var a in abilities)
                    if (a != null) skills.Add(a.ToSkill());
            return new Combatant(characterId, displayName, isPlayer,
                maxHp, maxSp, attack, defense, magicAttack, magicDefense, agility,
                affinities, skills);
        }
    }
}
