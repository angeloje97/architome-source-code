using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class ItemSlotHandler : MonoBehaviour
    {
        // Start is called before the first frame update

        public Action<ItemEventData> OnChangeItem;
    }

    public struct ItemEventData
    {
        public ItemInfo newItem;
        public InventorySlot itemSlot;
    }
}