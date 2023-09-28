using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<Item> Items;

        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public event Action<Dictionary<int, Item>> OnInventoryUpdated;

        public void Initialize()
        {
            Items = new List<Item>();
            for (int i = 0; i < Size; i++)
            {
                Items.Add(Item.GetEmptyItem());
            }
        }

        public void AddItem(ItemSo item, int quantity)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].IsEmpty)
                {
                    Items[i] = new Item()
                    {
                        item = item,
                        quantity = quantity
                    };
                    return;
                }
            }
        }

        public void AddItem(Item item)
        {
            AddItem(item.item, item.quantity);
        }

        public Dictionary<int, Item> GetCurrentInventoryState()
        {
            Dictionary<int, Item> returnValue =
                new Dictionary<int, Item>();

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].IsEmpty)
                    continue;
                returnValue[i] = Items[i];
            }
            return returnValue;
        }

        public Item GetItemAt(int itemIndex)
        {
            return Items[itemIndex];
        }

        public void SwapItems(int itemIndex_1, int itemIndex_2)
        {
            Item item1 = Items[itemIndex_1];
            Items[itemIndex_1] = Items[itemIndex_2];
            Items[itemIndex_2] = item1;
            InformAboutChange();
        }

        private void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }
    }

    [Serializable]
    public struct Item
    {
        public int quantity;
        public ItemSo item;
        public bool IsEmpty => item == null;

        public Item ChangeQuantity(int newQuantity)
        {
            return new Item
            {
                item = this.item,
                quantity = newQuantity,
            };
        }

        public static Item GetEmptyItem()
            => new Item
            {
                item = null,
                quantity = 0,
            };
    }
}