using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentSlot : InventorySlot
    {
        public Augment augments;
        public AbilityInfo ability;
        void Start()
        {
            GetDependencies();
        }

        protected override void GetDependencies()
        {
            base.GetDependencies();
            AddListenerPredicate(InventorySlotEvent.OnCanInsertCheck, HandleCanInsertCheck, this);

        }
        void Update()
        {
            HandleEvents();
        }

        public int SlotIndex()
        {
            var allSlots = transform.parent.GetComponentsInChildren<AugmentSlot>().ToList();

            return allSlots.IndexOf(this);
        }

        public int GroupSize()
        {
            var allSlots = transform.parent.GetComponentsInChildren<AugmentSlot>();
            return allSlots.Length;
        }

        public void SetAbility(AbilityInfo ability)
        {
            this.ability = ability;
        }

        bool HandleCanInsertCheck((InventorySlot, ItemInfo) data)
        {
            var item = data.Item2;
            if (item == null) return false;
            if (item.item == null) return false;
            if (item.item.GetType() != typeof(AugmentItem)) return false;
            if (EntityInCombat())
            {
                var itemEventData = new ItemEventData() { newItem = item, itemSlot = this };
                itemEventData.SetMessage("Can't insert augment during combat");
                itemSlotHandler.Invoke(eItemEvent.OnCantInsert, itemEventData);
                return false;
            }

            var augmentItem = (AugmentItem)item.item;
            return augmentItem.CanAttachTo(ability);
        }

        public override bool CanRemoveFromSlot(ItemInfo item)
        {
            if (!base.CanRemoveFromSlot(item)) return false;

            if (EntityInCombat())
            {
                var itemEvent = new ItemEventData(item) { itemSlot = this };
                itemEvent.SetMessage("Can't remove augment during combat");

                itemSlotHandler.Invoke(eItemEvent.OnCantInsert, itemEvent);

                return false;
            }
            return true;
        }

        public bool EntityInCombat()
        {
            if (ability == null) return false;
            if (ability.entityInfo == null) return false;
            if (!ability.entityInfo.isInCombat) return false;

            return true;
        }
    }
}
