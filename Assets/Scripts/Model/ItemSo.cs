using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public enum ArmorType
    {
        None,
        Head,
        Face,
        Body,
        LeftHand,
        RightHand,
        Ring1,
        Ring2,
        Necklace,
        Feet
    }
    [CreateAssetMenu]
    public class ItemSo : ScriptableObject
    {
        [field: SerializeField]
        public ArmorType ArmorSlot { get; private set; } = ArmorType.None;

        [field: SerializeField]
        public int DefenseValue { get; private set; } = 0;
        [field: SerializeField]
        public bool isStackable { get; set; }
        [field:SerializeField]
        public int ID { get; private set; }

        [field: SerializeField]
        public int MaxStackSize { get; set; } = 1;

        [field: SerializeField]
        public string Name { get; set; }


        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }

        [field: SerializeField]
        public Sprite ItemImage { get; set; }

    }
}