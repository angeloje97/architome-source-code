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
        [Multiline]
        public string itemDescription;
        public ItemType itemType;
        public int maxStacks;
        public float goldValue = 1f;
        public GameObject itemObject;

        public virtual void Use(UseData data)
        {

        }

        public virtual string Description()
        {
            return $"{itemDescription}";
        }

        public virtual string Attributes()
        {
            return "";
        }
        
        public virtual string Requirements()
        {
            return "";
        }

        public virtual string SubHeadline()
        {
            return "";
        }
        public virtual string Value()
        {
            var value = "";

            if (goldValue > 0)
            {
                value += $"{goldValue} gold\n";
            }

            return value;
        }

        public void SetId(int id, bool forceSet = false)
        {
            if (idSet && !forceSet) return;
            idSet = true;
            this.id = id;
        }

        public virtual ToolTipData ToolTipData()
        {
            return new()
            {
                icon = itemIcon,
                name = itemName,
                enableRarity = true,
                type = rarity.ToString(),
                subeHeadline = SubHeadline(),
                attributes = Attributes(),
                requirements = Requirements(),
                description = Description(),
                value = Value()
            };
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

        public static bool Equipable(Item item)
        {
            return typeof(Equipment).IsAssignableFrom(item.GetType());
        }

        public static bool Useable(Item item)
        {
            if (item.GetType() == typeof(Consumable)) return true;
            return false;
        }
    }

    [Serializable]
    public class ItemData
    {
        public static ItemData Empty { get { return new ItemData() { item = null, amount = 0 }; } }

        public ItemData(ItemInfo info)
        {
            if (info == null)
            {
                item = null;
                amount = 0;
                return;
            }
            this.item = info.item;
            this.amount = info.currentStacks;
        }

        public ItemData() { }

        public Item item;
        public int amount;
    }

    public class UseData
    {
        public ItemInfo itemInfo;
        public EntityInfo entityUsed;
    }
}