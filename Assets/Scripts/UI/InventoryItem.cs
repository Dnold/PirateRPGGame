using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace Inventory.UI
{
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField]
        private Image itemImage;
        [SerializeField]
        TMP_Text quantityText;

        [SerializeField]
        private Image borderImage;

        public event Action<InventoryItem> OnItemClicked, OnItemDropped, OnItemBeginDrag, OnItemEndDrag, OnRightMouseBtnClick;

        private bool empty = true;
        public void Awake()
        {
            ResetData();
            Deselect();
        }
        public void ResetData()
        {
            itemImage.gameObject.SetActive(false);
            empty = true;
        }
        public void Deselect()
        {
            borderImage.enabled = false;

        }
        public void SetData(Sprite sprite, int quantity)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            quantityText.text = quantity.ToString();
            empty = false;
        }
        public void Select()
        {
            borderImage.enabled = true;
            borderImage.gameObject.SetActive(true);

        }
        public void OnBeginDrag()
        {
            if (!empty)
            {
                OnItemBeginDrag?.Invoke(this);
            }
        }
        public void OnDrop()
        {
            OnItemDropped?.Invoke(this);
        }
        public void OnEndDrag()
        {
            OnItemEndDrag?.Invoke(this);
        }
        public void OnPointerClick(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                OnRightMouseBtnClick?.Invoke(this);
            }
            else if (pointerEventData.button == PointerEventData.InputButton.Left)
            {
                OnItemClicked?.Invoke(this);
            }
        }
    }
}