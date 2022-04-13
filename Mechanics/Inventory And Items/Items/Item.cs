using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;


namespace Architome
{
    [CreateAssetMenu(fileName = "arch_item", menuName = "Item")]
    public class Item : ScriptableObject
    {
        // Start is called before the first frame update


        [Header("Item Info")]
        public string itemName;
        public Sprite itemIcon;
        public int itemID;
        public Rarity rarity;
        public bool playerObtainable;
        [Multiline]
        public string itemDescription;
        public ItemType itemType;
        public int maxStacks;
        public GameObject itemObject;

        public static bool IsEquipment(Item current)
        {
            if (current == null) { return false; }

            var value = current.GetType();

            if (value == typeof(Equipment)) { return true; }

            return false;

        }

        public static bool IsWeapon(Item current)
        {
            if (current == null) { return false; }
            var value = current.GetType();
            if (value == typeof(Weapon)) { return true; }
            return false;
        }



    }
}