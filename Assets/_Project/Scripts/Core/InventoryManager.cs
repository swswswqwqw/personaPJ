using System;
using System.Collections.Generic;
using UnityEngine;
using Amane.Data;

namespace Amane.Core
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int count;

        public InventorySlot(ItemData item, int count)
        {
            this.item = item;
            this.count = count;
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private int maxSlots = 64;
        [SerializeField] private int startingMoney = 5000;

        private readonly List<InventorySlot> slots = new();
        public int Money { get; private set; }

        public IReadOnlyList<InventorySlot> Slots => slots;

        public event Action<ItemData, int> OnItemAdded;
        public event Action<ItemData, int> OnItemRemoved;
        public event Action<int> OnMoneyChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            Money = startingMoney;
        }

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            var existing = slots.Find(s => s.item == item);
            if (existing != null)
            {
                int newCount = Mathf.Min(existing.count + amount, item.maxStack);
                int added = newCount - existing.count;
                if (added <= 0) return false;
                existing.count = newCount;
                OnItemAdded?.Invoke(item, added);
                return true;
            }

            if (slots.Count >= maxSlots) return false;

            slots.Add(new InventorySlot(item, Mathf.Min(amount, item.maxStack)));
            OnItemAdded?.Invoke(item, amount);
            return true;
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            var existing = slots.Find(s => s.item == item);
            if (existing == null || existing.count < amount) return false;

            existing.count -= amount;
            if (existing.count <= 0) slots.Remove(existing);
            OnItemRemoved?.Invoke(item, amount);
            return true;
        }

        public int GetItemCount(ItemData item)
        {
            var existing = slots.Find(s => s.item == item);
            return existing?.count ?? 0;
        }

        public bool HasItem(ItemData item, int amount = 1)
        {
            return GetItemCount(item) >= amount;
        }

        public List<InventorySlot> GetItemsByCategory(ItemCategory category)
        {
            return slots.FindAll(s => s.item.category == category);
        }

        public List<InventorySlot> GetUsableItemsInBattle()
        {
            return slots.FindAll(s => s.item.category == ItemCategory.Consumable && s.item.usableInBattle);
        }

        public bool SpendMoney(int amount)
        {
            if (amount <= 0 || Money < amount) return false;
            Money -= amount;
            OnMoneyChanged?.Invoke(Money);
            return true;
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;
            Money += amount;
            OnMoneyChanged?.Invoke(Money);
        }
    }
}
