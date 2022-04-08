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
        void Start()
        {
            slot = GetComponentInParent<InventorySlot>();
            
            slot.events.OnItemChange += OnItemChange;
            slot.events.OnSetSlot += OnSetSlot;
        }

        // Update is called once per frame

        void OnItemChange(InventorySlot slot, Item previous, Item after)
        {
            var isTaken = after != null;
            var image = GetComponent<Image>();

            image.enabled = !isTaken;
        }

        void OnSetSlot(InventorySlot slot)
        {
            var isTaken = slot.item != null;

            var image = GetComponent<Image>();

            image.enabled = !isTaken;
        }
    }


}