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

        public override bool CanInsert(ItemInfo item)
        {
            if (item == null) return false;
            if (item.item == null) return false;
            if (item.item.GetType() != typeof(AugmentItem)) return false;
            if (EntityInCombat())
            {
                itemSlotHandler.OnCantInsertToSlot?.Invoke(this, item, "Can't insert augment during combat.");
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
                itemSlotHandler.OnCantInsertToSlot?.Invoke(this, item, "Can't remove augment during combat");

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
