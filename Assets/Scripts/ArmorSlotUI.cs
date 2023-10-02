using Inventory.Model;
using System;
using UnityEngine.EventSystems;
using UnityEngine;

public class ArmorSlotUI : MonoBehaviour, IPointerClickHandler
{
    public event Action<ArmorType> OnRightClickArmor, OnLeftClickArmor;
    

    [SerializeField]
    private ArmorType armorType;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClickArmor?.Invoke(armorType);
        }
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClickArmor?.Invoke(armorType);
        }
    }
}
