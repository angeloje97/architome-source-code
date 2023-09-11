using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public enum ItemEvents
    {
        OnStartDrag,
        OnEndDrag,
    }


    public class ItemEventHandler : MonoBehaviour
    {
        public static ItemEventHandler active;
        private void Awake()
        {
            SingletonManger.HandleSingleton(GetType(), gameObject, true, onSuccess: () => {
                active = this;
                events = new(this);
            });
        }

        ArchEventHandler<ItemEvents, ItemInfo> events;
        public ItemInfo currentItemHolding { get; private set; }

        public static void AddListener(ItemEvents eventType, Action<ItemInfo> action, Component listener)
        {
            if (active == null) return;
            active.events.AddListener(eventType, action, listener);
        }

        public static void InvokeEvent(ItemEvents eventType, ItemInfo eventData)
        {
            if (active == null) return;
            active.events.Invoke(eventType, eventData);
        }

        public static async void HoldItem(ItemInfo item)
        {
            if (active == null) return;
            active.currentItemHolding = item;
            InvokeEvent(ItemEvents.OnStartDrag, item);

            await item.EndDragging();

            active.currentItemHolding = null;

            InvokeEvent(ItemEvents.OnEndDrag, item);

        }
    }
}
