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
        public ItemFX effects;
        public bool usingDefaultFX;
        public Rarity rarity;
        [Multiline]
        public string itemDescription;
        public ItemType itemType;
        public float goldValue = 1f;
        public GameObject itemObject;

        public int MaxStacks { get { return maxStacks; } }

        [SerializeField] protected int maxStacks;
        [SerializeField] protected bool infiniteStacks;

        private void OnValidate()
        {
            if (!infiniteStacks) return;
            infiniteStacks = false;
        }

        public virtual void Use(UseData data)
        {

        }

        public override string ToString()
        {
            return itemName;
        }

        public bool Equals(Item other)
        {

            return other._id == _id;
        }

        public virtual string Description()
        {
            return $"{itemDescription}";
        }

        public virtual string Attributes()
        {
            return "";
        }

        public virtual int NewStacks(int currentStacks, int stacksToAdd, out int leftover)
        {
            if (infiniteStacks)
            {
                leftover = 0;
                return currentStacks + stacksToAdd;
            }

            if (currentStacks + stacksToAdd > maxStacks)
            {
                leftover = currentStacks + stacksToAdd - maxStacks;
                return maxStacks;

            }

            leftover = 0;

            return currentStacks + stacksToAdd;
        }

        public virtual bool ValidStacks(int countCheck)
        {
            if (infiniteStacks) return true;
            return countCheck <= maxStacks;
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

        public virtual ToolTipData ToolTipData(int amount = 1)
        {
            var name = amount > 1 ? $"{itemName} x{amount}" : itemName;

            return new()
            {
                icon = itemIcon,
                name = name,
                enableRarity = true,
                rarity = rarity,
                type = $"{rarity}",
                subeHeadline = SubHeadline(),
                attributes = Attributes(),
                requirements = Requirements(),
                description = Description(),
                value = Value()
            };
        }


        public virtual bool IsCurrency()
        {
            return false;
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
        public ItemData() 
        {
            item = null;
            amount = 0;
        }

        public Item item;
        public int amount;
    }

    public class UseData
    {
        public ItemInfo itemInfo;
        public EntityInfo entityUsed;
        public GuildManager guildManager;
    }
}