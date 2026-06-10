using System;
using UnityEngine;

namespace ArcadiaOfEchoes.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "ArcadiaOfEchoes/Item")]
    public class ItemData : ScriptableObject
    {
        public string ItemId;
        public string ItemName;
        [TextArea] public string Description;
        public ItemType Type;
        public int BuyPrice;
        public int SellPrice;
        public bool IsKeyItem;

        [Header("回復アイテム")]
        public int HPRestore;
        public int SPRestore;
        public StatusEffect CureEffect;

        [Header("戦闘アイテム")]
        public ElementType DamageElement;
        public int DamageAmount;
        public SkillTarget Target;

        [Header("能力強化アイテム")]
        public SocialStatType StatToBoost;
        public int StatBoostAmount;
    }

    public enum ItemType
    {
        Consumable,
        BattleItem,
        KeyItem,
        Equipment,
        Material,
        StatBoost
    }
}
