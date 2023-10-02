using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class EquippedArmorUI : MonoBehaviour
{
    [SerializeField] private Image headSlot;
    [SerializeField] private Image faceSlot;
    [SerializeField] private Image bodySlot;
    [SerializeField] private Image leftHandSlot;
    [SerializeField] private Image rightHandSlot;
    [SerializeField] private Image leftRingSlot;
    [SerializeField] private Image rightRingSlot;
    [SerializeField] private Image necklaceSlot;
    [SerializeField] private Image feetSlot;
    public void UpdateArmorSlot(ArmorType armorType, Sprite armorSprite)
    {
        switch (armorType)
        {
            case ArmorType.Head:
                headSlot.sprite = armorSprite;
                headSlot.enabled = (armorSprite != null);
                break;
            case ArmorType.Face:
                faceSlot.sprite = armorSprite;
                faceSlot.enabled = (armorSprite != null);
                break;
            case ArmorType.Body:
                bodySlot.sprite = armorSprite;
                bodySlot.enabled = (armorSprite != null);
                break;
            case ArmorType.LeftHand:
                leftHandSlot.sprite = armorSprite;
                leftHandSlot.enabled = (armorSprite != null);
                break;
            case ArmorType.RightHand:
                rightHandSlot.sprite = armorSprite;
                rightHandSlot.enabled = (armorSprite != null);
                break;
            case ArmorType.Ring1:
                leftRingSlot.sprite = armorSprite;
                leftRingSlot.enabled = (armorSprite != null);
                break;
            case ArmorType.Ring2:
                rightRingSlot.sprite = armorSprite;
                rightRingSlot.enabled = (armorSprite != null);
                break;
            case ArmorType.Necklace:
                necklaceSlot.sprite = armorSprite;
                necklaceSlot.enabled = (armorSprite != null);
                break;
            case ArmorType.Feet:
                feetSlot.sprite = armorSprite;
                feetSlot.enabled = (armorSprite != null);
                break;
        }
    }

    public void ResetAllSlots()
    {
        headSlot.enabled = false;
        faceSlot.enabled = false;
        bodySlot.enabled = false;
        leftHandSlot.enabled = false;
        rightHandSlot.enabled = false;
        leftRingSlot.enabled = false;
        rightRingSlot.enabled = false;
        necklaceSlot.enabled = false;
        feetSlot.enabled = false;
    }
}


