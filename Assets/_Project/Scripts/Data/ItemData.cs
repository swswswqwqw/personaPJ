using UnityEngine;

namespace EchoesOfArcadia.Data
{
    public enum ItemType
    {
        Consumable,
        Equipment,
        KeyItem,
        Material
    }

    public enum EquipSlot
    {
        None,
        Weapon,
        Armor,
        Accessory
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "EchoesOfArcadia/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("基本情報")]
        public string itemName;
        public string furigana;
        [TextArea(1, 3)] public string description;
        public ItemType itemType;
        public int price;
        public int maxStack = 99;

        [Header("消費アイテム効果")]
        public int healHP;
        public int healSP;
        public StatusEffect cureStatus;
        public bool revive;

        [Header("装備")]
        public EquipSlot equipSlot;
        public int attackBonus;
        public int defenseBonus;
        public int magicBonus;
        public int agilityBonus;
        public Element elementalBonus;
    }
}
