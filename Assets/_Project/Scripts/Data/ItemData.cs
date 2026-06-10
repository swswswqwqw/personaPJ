using UnityEngine;

namespace Astra.Data
{
    public enum ItemType
    {
        Consumable,
        Equipment,
        KeyItem,
        Material
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "Astra/Data/ItemData")]
    public class ItemData : ScriptableObject
    {
        [Header("基本情報")]
        public string itemName;
        [TextArea(1, 3)] public string description;
        public ItemType itemType;
        public int price;
        public int maxStack = 99;

        [Header("消費アイテム効果")]
        public int hpRestore;
        public int spRestore;
        public string statusCure;

        [Header("装備効果")]
        public int strengthBonus;
        public int magicBonus;
        public int enduranceBonus;
        public int agilityBonus;
        public int luckBonus;

        [Header("ビジュアル")]
        public Sprite icon;
    }
}
