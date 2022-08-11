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

            var augmentItem = (AugmentItem)item.item;
            return augmentItem.CanAttachTo(ability);



        }
    }
}
