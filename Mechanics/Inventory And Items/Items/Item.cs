using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using System;


namespace Architome
{
    [CreateAssetMenu(fileName = "arch_item", menuName = "Item")]
    public class Item : ScriptableObject
    {
        // Start is called before the first frame update
        [SerializeField] int id;
        public int _id
        {
            get { return idSet ? id: 99999999; }
            private set { id = value; }
        }

        public bool idSet;


        [Header("Item Info")]
        public string itemName;
        public Sprite itemIcon;
        public Rarity rarity;
        public bool playerObtainable;
        [Multiline]
        public string itemDescription;
        public ItemType itemType;
        public int maxStacks;
        public GameObject itemObject;


        public void SetId(int id, bool forceSet = false)
        {
            if (idSet && !forceSet) return;
            idSet = true;
            this.id = id;
        }

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

    [Serializable]
    public class ItemData
    {
        public Item item;
        public int amount;
    }
}