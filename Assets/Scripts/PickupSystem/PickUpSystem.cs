using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;
using Inventory;
using System.Linq;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    public InventorySO Inventory;
    public ItemSo[] allItems;
    public EquippedArmorSO equippedArmor;

    public void AddItem(int itemID, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            Inventory.AddItem(GetItemByID(itemID), 1);
        }

    }
    public void ClearInventory()
    {
        Inventory.Clear();
        equippedArmor.ClearArmor();
    }

    public ItemSo GetItemByID(int id)
    {
        return allItems.FirstOrDefault(x => x.ID == id);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
