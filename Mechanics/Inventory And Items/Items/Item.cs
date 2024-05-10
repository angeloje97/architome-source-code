using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using System;


namespace Architome
{

    [CreateAssetMenu(fileName = "New Item", menuName = "Architome/Item/New Item")]
    
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


        [Header("Item Properties")]
        public string itemName;
        public Sprite itemIcon;
        public ItemFX effects;
        public bool usingDefaultFX;
        public Rarity rarity;
        [Multiline]
        public string itemDescription;
        public ItemType itemType;
        public float value = 1f;
        public Currency currencyExchange;
        public GameObject itemObject;
        public int spriteIndex = -1;
        

        public int MaxStacks { get { return maxStacks; } }

        
        [SerializeField, Min(1)] protected int maxStacks;
        [SerializeField] protected bool infiniteStacks;

        private void OnValidate()
        {
            if(itemName == null || itemName.Length == 0)
            {
                itemName = name;
            }
        }

        public virtual void AdjustValue()
        {
            if(rarity == 0)
            {
                rarity = Rarity.Poor;
            }
        }


        #region Use
        public virtual bool Useable(UseData data)
        {
            return false;
        }

        public virtual string UseString()
        {
            return "Use";
        }

        public virtual void Use(UseData data)
        {

        }
        #endregion

        public override string ToString()
        {
            return itemName;
        }

        public virtual bool Equals(Item other)
        {

            return other._id == _id;
        }

        #region Tooltip

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
        public virtual string Value(int amount)
        {
            var result = "";


            var totalValue = amount * value;

            Debugger.UI(7568, $"Total Value {totalValue}");


            if (totalValue > 0)
            {
                var primeValues = Economy.active.PrimeValues(totalValue);

                Debugger.UI(7569, $"{primeValues.Count}");
                foreach (var data in primeValues)
                {
                    var sprite = data.item.spriteIndex != -1 ? $"<sprite={data.item.spriteIndex}>" : data.item.ToString()[..1];
                    result += $"{data.amount} {sprite}";
                }
            }

            return result;
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
                value = Value(amount)
            };
        }

        #endregion
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
        
        
        public void SetId(int id, bool forceSet = false)
        {
            if (idSet && !forceSet) return;
            idSet = true;
            this.id = id;
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


        public void SetMaxStacks(int newStacks)
        {
            maxStacks = newStacks;
        }
    }

    [Serializable]
    public class ItemData
    {
        public Item item;
        public int amount;
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

        public ItemData(MerchData data, int amount = 1)
        {
            item = data.item;
            this.amount = amount;
        }

        public ItemData(Item item, int amount)
        {
            this.item = item;
            this.amount = amount;
        }

        public ItemData() 
        {
            item = null;
            amount = 0;
        }

    }

    public class UseData
    {
        public ItemInfo itemInfo;
        public EntityInfo entityUsed;
        public GuildManager guildManager;
        public List<InventorySlot> slots;
        public Transform targetParent;
        public ItemSlotHandler slotHandler;
        public bool triggerEvent;
        public ItemEvent eventType;
    }
}