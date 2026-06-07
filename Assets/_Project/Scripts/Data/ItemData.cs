using UnityEngine;

namespace EchoesOfArcadia.Data
{
    public enum ItemCategory
    {
        Consumable,
        Equipment,
        Material,
        KeyItem
    }

    public enum ItemEffect
    {
        None,
        HealHP,
        HealSP,
        HealHPSP,
        CureStatus,
        ReviveAlly,
        AttackBoost,
        DefenseBoost,
        SpeedBoost,
        AllStatsBoost
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "EchoesOfArcadia/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("基本情報")]
        public string itemName;
        [TextArea(1, 3)] public string description;
        public ItemCategory category;
        public int buyPrice;
        public int sellPrice;
        public int maxStack = 99;

        [Header("使用効果")]
        public ItemEffect effect = ItemEffect.None;
        public int effectValue;
        public TargetType targetType = TargetType.SingleAlly;
        public bool usableInBattle = true;
        public bool usableInField = true;

        [Header("装備")]
        public int attackBonus;
        public int defenseBonus;
        public int magicBonus;
        public int agilityBonus;
    }
}
