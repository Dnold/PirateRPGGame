using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory;

[CreateAssetMenu]
public class EquippedArmorSO : ScriptableObject
{
    public ItemSo Head, Face, Body, LeftHand, RightHand, Ring1, Ring2, Necklace, Feet;

    public ItemSo EquipArmor(ItemSo newArmor, ArmorType armorType)
    {
        ItemSo currentArmor = null;

        switch (armorType)
        {
            case ArmorType.Head:
                currentArmor = Head;
                Head = newArmor;
                break;

            case ArmorType.Face:
                currentArmor = Face;
                Face = newArmor;
                break;

            case ArmorType.Body:
                currentArmor = Body;
                Body = newArmor;
                break;

            case ArmorType.LeftHand:
                currentArmor = LeftHand;
                LeftHand = newArmor;
                break;

            case ArmorType.RightHand:
                currentArmor = RightHand;
                RightHand = newArmor;
                break;

            case ArmorType.Ring1:
                currentArmor = Ring1;
                Ring1 = newArmor;
                break;

            case ArmorType.Ring2:
                currentArmor = Ring2;
                Ring2 = newArmor;
                break;

            case ArmorType.Necklace:
                currentArmor = Necklace;
                Necklace = newArmor;
                break;

            case ArmorType.Feet:
                currentArmor = Feet;
                Feet = newArmor;
                break;
        }

        return currentArmor;
    }
    public void ClearArmor()
    {
        Head = null;
        Face = null;
        LeftHand = null;
        RightHand = null;
        Ring1 = null;
        Ring2 = null;
        Necklace = null;
        Feet = null;

    }
    public ItemSo UnequipItem(ArmorType armorType)
    {
        switch (armorType)
        {
            case ArmorType.Head:
                ItemSo tempHead = Head;
                Head = null;
                return tempHead;

            case ArmorType.Face:
                ItemSo tempFace = Face;
                Face = null;
                return tempFace;

            case ArmorType.Body:
                ItemSo tempBody = Body;
                Body = null;
                return tempBody;

            case ArmorType.LeftHand:
                ItemSo tempLeftHand = LeftHand;
                LeftHand = null;
                return tempLeftHand;

            case ArmorType.RightHand:
                ItemSo tempRightHand = RightHand;
                RightHand = null;
                return tempRightHand;

            case ArmorType.Ring1:
                ItemSo tempRing1 = Ring1;
                Ring1 = null;
                return tempRing1;

            case ArmorType.Ring2:
                ItemSo tempRing2 = Ring2;
                Ring2 = null;
                return tempRing2;

            case ArmorType.Necklace:
                ItemSo tempNecklace = Necklace;
                Necklace = null;
                return tempNecklace;

            case ArmorType.Feet:
                ItemSo tempFeet = Feet;
                Feet = null;
                return tempFeet;

            default:
                return null;  // Return null if the armor type doesn't match any case

        }
    }
    public Item GetArmorItem(ArmorType armorType)
    {
        switch (armorType)
        {
            case ArmorType.Head:
                return new Item { item = Head, quantity = 1 };
            case ArmorType.Face:
                return new Item { item = Face, quantity = 1 };
            case ArmorType.Body:
                return new Item { item = Body, quantity = 1 };
            case ArmorType.LeftHand:
                return new Item { item = LeftHand, quantity = 1 };
            case ArmorType.RightHand:
                return new Item { item = RightHand, quantity = 1 };
            case ArmorType.Ring1:
                return new Item { item = Ring1, quantity = 1 };
            case ArmorType.Ring2:
                return new Item { item = Ring2, quantity = 1 };
            case ArmorType.Necklace:
                return new Item { item = Necklace, quantity = 1 };
            case ArmorType.Feet:
                return new Item { item = Feet, quantity = 1 };
            default:
                return Item.GetEmptyItem();
        }
    
}
public int CalculateTotalDefense()
{
    int total = 0;
    total += Head?.DefenseValue ?? 0;
    total += Face?.DefenseValue ?? 0;
    total += Body?.DefenseValue ?? 0;
    total += Ring1?.DefenseValue ?? 0;
    total += Ring2?.DefenseValue ?? 0;
    total += Necklace?.DefenseValue ?? 0;
    total += Feet?.DefenseValue ?? 0;
    return total;
}
}
