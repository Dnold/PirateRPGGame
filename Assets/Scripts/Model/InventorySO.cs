using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject
    {
        [SerializeField]
        private List<Item> Items;

        [field: SerializeField]
        public int Size { get; private set; }

        public event Action<Dictionary<int, Item>> OnInventoryUpdated;

        public void Initialize()
        {
            if (Items.Count != Size)
            {
                Items = new List<Item>();
                for (int i = 0; i < Size; i++)
                {
                    
                    Items.Add(Item.GetEmptyItem());
                }
            }
        }
        public void Clear()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i] = Item.GetEmptyItem();
            }
            InformAboutChange();
        }
        public int AddItem(ItemSo item, int quantity)
        {
            if (!item.isStackable)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    while (quantity > 0 && !IsInventoryFull())
                    {

                        quantity -= AddItemToFirstFreeSlot(item, 1);
                        InformAboutChange();
                        return quantity;
                    }

                }
            }

            quantity = AddStackableItem(item, quantity);
            InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSo item, int v)
        {
            Item newItem = new Item
            {
                item = item,
                quantity = v
            };
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].IsEmpty)
                {
                    Items[i] = newItem;
                    return v;
                }
            }
            return 0;
        }

        public bool IsInventoryFull() => Items.Where(e => e.IsEmpty).Any() == false;


        private int AddStackableItem(ItemSo item, int quantity)
        {
            for (int i = 0; i < Items.Count; i++)
            {

                if (Items[i].IsEmpty)
                {
                    continue;
                }
                if (Items[i].item.ID == item.ID)
                {
                    int amountToTake = Items[i].item.MaxStackSize - Items[i].quantity;
                    if (quantity > amountToTake)
                    {
                        Items[i] = Items[i].ChangeQuantity(Items[i].item.MaxStackSize);
                        quantity -= amountToTake;

                    }
                    else
                    {
                        Items[i] = Items[i].ChangeQuantity(Items[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }
            while (quantity > 0 && IsInventoryFull() == false)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item, newQuantity);
            }
            return quantity;
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
        public int IndexOf(Item item)
        {
            return Items.IndexOf(item);
        }

        public void SwapItems(int itemIndex_1, int itemIndex_2)
        {
            Item item1 = Items[itemIndex_1];
            Items[itemIndex_1] = Items[itemIndex_2];
            Items[itemIndex_2] = item1;
            InformAboutChange();
        }
        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                Items[index] = Item.GetEmptyItem(); // Set the item to an empty item
                InformAboutChange();
            }
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