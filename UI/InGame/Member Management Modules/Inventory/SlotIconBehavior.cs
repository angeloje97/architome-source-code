using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class SlotIconBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        public InventorySlot slot;

        public struct Info
        {
            public Image slotIconPlaceHolder;
            public Image slotRarity;

        }

        void Start()
        {
            slot = GetComponentInParent<InventorySlot>();
            
            slot.events.OnItemChange += OnItemChange;
            slot.AddListener(InventorySlotEvent.OnSetSlot, OnSetSlot, this);
        }

        // Update is called once per frame

        void OnItemChange(InventorySlot slot, Item previous, Item after)
        {

            var isTaken = after != null;
            var image = GetComponent<Image>();
            image.enabled = !isTaken;

        }

        void OnSetSlot((InventorySlot, ItemInfo) data)
        {
            var slot = data.Item1;
            ArchAction.Yield(() => {
                var isTaken = slot.item != null;

                var image = GetComponent<Image>();

                image.enabled = !isTaken;
            });
        }
    }


}