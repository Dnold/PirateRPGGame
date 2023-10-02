using Inventory.Model;
using Inventory.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField]
        private InventoryPage inventoryUI;

        [SerializeField]
        private InventorySO inventoryData;
        [SerializeField]
        private EquippedArmorSO equippedArmor;

        [SerializeField]
        private EquippedArmorUI equippedArmorUI;

        public List<Item> initialItems = new List<Item>();

        private void Start()
        {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            //inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (Item item in initialItems)
            {
                if (item.IsEmpty)
                    continue;
                inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, Item> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage,
                    item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItems += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
            inventoryUI.OnArmorUnequip += UnequipAndAddToInventory;
            inventoryUI.OnArmorSelected += SelectArmor;


        }
        public void SelectArmor(ArmorType armor)
        {
            Item armorItem = equippedArmor.GetArmorItem(armor);
            inventoryUI.UpdateDescription(inventoryData.IndexOf(armorItem), armorItem.item.ItemImage, armorItem.item.Name, armorItem.item.Description);
        }

        public void UnequipAndAddToInventory(ArmorType armorType)
        {
            // Unequip the armor item


            // Check if there was an item to unequip

            if (!inventoryData.IsInventoryFull())
            {
                ItemSo unequippedItem = equippedArmor.UnequipItem(armorType);
                if (unequippedItem != null)
                {

                    // Add the unequipped item back to the inventory
                    inventoryData.AddItem(unequippedItem, 1);
                }
            }
            UpdateEquippedArmorUI();
        }




        private void HandleItemActionRequest(int itemIndex)
        {

            ItemSo itemToEquip = inventoryData.GetItemAt(itemIndex).item;

            if (itemToEquip.ArmorSlot != ArmorType.None)
            {
                EquipArmor(itemToEquip, itemToEquip.ArmorSlot);
                inventoryData.RemoveItemAt(itemIndex);
            }

        }


        private void HandleDragging(int itemIndex)
        {
            Item inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
        {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            Item inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSo item = inventoryItem.item;
            inventoryUI.UpdateDescription(itemIndex, item.ItemImage,
                item.Name, item.Description);
        }


        public void EquipArmor(ItemSo armorToEquip, ArmorType armorType)
        {
            // Equip the new armor and get the currently equipped armor
            ItemSo armorToUnequip = equippedArmor.EquipArmor(armorToEquip, armorType);

            // If there was armor already equipped in that slot, add it back to the inventory
            if (armorToUnequip != null)
            {
                inventoryData.AddItem(armorToUnequip, 1);
            }
            UpdateEquippedArmorUI();
        }
        public void UpdateEquippedArmorUI()
        {
            if (equippedArmor.Head != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.Head, equippedArmor.Head.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.Head, null);

            if (equippedArmor.Face != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.Face, equippedArmor.Face.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.Face, null);

            if (equippedArmor.Body != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.Body, equippedArmor.Body.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.Body, null);

            if (equippedArmor.LeftHand != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.LeftHand, equippedArmor.LeftHand.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.LeftHand, null);

            if (equippedArmor.RightHand != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.RightHand, equippedArmor.RightHand.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.RightHand, null);

            if (equippedArmor.Ring1 != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.Ring1, equippedArmor.Ring1.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.Ring1, null);

            if (equippedArmor.Ring2 != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.Ring2, equippedArmor.Ring2.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.Ring2, null);

            if (equippedArmor.Necklace != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.Necklace, equippedArmor.Necklace.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.Necklace, null);

            if (equippedArmor.Feet != null)
                equippedArmorUI.UpdateArmorSlot(ArmorType.Feet, equippedArmor.Feet.ItemImage);
            else
                equippedArmorUI.UpdateArmorSlot(ArmorType.Feet, null);

        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (inventoryUI.isActiveAndEnabled == false)
                {
                    inventoryUI.Show();
                    UpdateEquippedArmorUI();
                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryUI.UpdateData(item.Key,
                            item.Value.item.ItemImage,
                            item.Value.quantity);
                    }
                }
                else
                {
                    inventoryUI.Hide();
                }

            }
        }
    }
}