using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.UI
{
    public class InventoryPage : MonoBehaviour
    {
        [SerializeField]
        private InventoryItem itemPrefab;

        [SerializeField]
        private RectTransform contentPanel;

        [SerializeField]
        private InventoryDescription itemDescription;

        [SerializeField]
        private MouseFollower mouseFollower;

        List<InventoryItem> listOfUIItems = new List<InventoryItem>();
        
        [Header("Equipped Armor UI")]
        [SerializeField] private ArmorSlotUI headSlotUI;
        [SerializeField] private ArmorSlotUI faceSlotUI;
        [SerializeField] private ArmorSlotUI bodySlotUI;
        [SerializeField] private ArmorSlotUI leftHandSlotUI;
        [SerializeField] private ArmorSlotUI rightHandSlotUI;
        [SerializeField] private ArmorSlotUI leftRingSlotUI;
        [SerializeField] private ArmorSlotUI rightRingSlotUI;
        [SerializeField] private ArmorSlotUI necklaceSlotUI;
        [SerializeField] private ArmorSlotUI feetSlotUI;

        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnDescriptionRequested,
                OnItemActionRequested,
                OnStartDragging;

        public event Action<int, int> OnSwapItems;

        public event Action<ArmorType> OnArmorUnequip, OnArmorSelected;

        private void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
            headSlotUI.OnRightClickArmor += HandleArmorRightClick;
            faceSlotUI.OnRightClickArmor += HandleArmorRightClick;
            bodySlotUI.OnRightClickArmor += HandleArmorRightClick;
            leftHandSlotUI.OnRightClickArmor += HandleArmorRightClick;
            rightHandSlotUI.OnRightClickArmor += HandleArmorRightClick;
            leftRingSlotUI.OnRightClickArmor += HandleArmorRightClick;
            rightRingSlotUI.OnRightClickArmor += HandleArmorRightClick;
            necklaceSlotUI.OnRightClickArmor += HandleArmorRightClick;
            feetSlotUI.OnRightClickArmor += HandleArmorRightClick;
            headSlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            faceSlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            bodySlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            leftHandSlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            rightHandSlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            leftRingSlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            rightRingSlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            necklaceSlotUI.OnLeftClickArmor += HandleArmorLeftClick;
            feetSlotUI.OnLeftClickArmor += HandleArmorLeftClick;

        }
        private void HandleArmorRightClick(ArmorType armorType)
        {
            OnArmorUnequip?.Invoke(armorType);
            Debug.Log($"Unequipped {armorType} armor");
        }
        private void HandleArmorLeftClick(ArmorType armorType)
        {
            OnArmorSelected?.Invoke(armorType);
        }

        public void InitializeInventoryUI(int inventorysize)
        {
            for (int i = 0; i < inventorysize; i++)
            {
                InventoryItem uiItem =
                    Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(contentPanel);
                listOfUIItems.Add(uiItem);
                uiItem.OnItemClicked += HandleItemSelection;
                uiItem.OnItemBeginDrag += HandleBeginDrag;
                uiItem.OnItemDropped += HandleSwap;
                uiItem.OnItemEndDrag += HandleEndDrag;
                uiItem.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }

        internal void ResetAllItems()
        {
            foreach (var item in listOfUIItems)
            {
                item.ResetData();
                item.Deselect();
            }
        }

        internal void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
        {
            itemDescription.SetDescription(itemImage, name, description);
            DeselectAllItems();
            listOfUIItems[itemIndex].Select();
        }

        public void UpdateData(int itemIndex,
            Sprite itemImage, int itemQuantity)
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }

        private void HandleShowItemActions(InventoryItem inventoryItemUI)
        {
            OnItemActionRequested?.Invoke(listOfUIItems.IndexOf(inventoryItemUI));
        }

        private void HandleEndDrag(InventoryItem inventoryItemUI)
        {
            ResetDraggedItem();
        }

        private void HandleSwap(InventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDrag(InventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
                return;
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
        }

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleItemSelection(InventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
                return;
            OnDescriptionRequested?.Invoke(index);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            ResetSelection();
        }

        public void ResetSelection()
        {
            itemDescription.ResetDescription();
            DeselectAllItems();
        }

        private void DeselectAllItems()
        {
            foreach (InventoryItem item in listOfUIItems)
            {
                item.Deselect();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ResetDraggedItem();
        }
    }
}
