using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Loot Box", menuName = "Architome/Item/New Loot Box")]
    public class LootBox : Item
    {
        public ItemPool itemPool;
        public ItemPool.RequestData requestData;
        public string itemPoolName;

        public override string UseString()
        {
            return "Open";
        }

        public override async void Use(UseData data)
        {
            var inventorySlots = data.slots;
            var itemInfo = data.itemInfo;
            var items = itemPool.ItemsFromCustom(itemPoolName, requestData);
            var itemSlotHandler = data.itemInfo.GetComponentInParent<ItemSlotHandler>();
            var availableSlots = itemSlotHandler.AvailableSlots;
            if (availableSlots < items.Count)
            {
                if(data.itemInfo.currentStacks == 1 && availableSlots == items.Count + 1)
                {

                }
                else
                {
                    return;
                }
            }

            var parent = data.targetParent;

            itemInfo.ReduceStacks();

            await Task.Delay(500);

            foreach (var item in items)
            {
                var createdItem = WorldActions.active.CreateItemUI(item, parent, true);
            }

            Debugger.UI(65491, $"Items in loot box {items.Count}");
        }

        public override string Attributes()
        {
            var lootPool = itemPool.ItemsFromName(itemPoolName);
            if (lootPool == null) return "";
            
            var possibleItems = lootPool.possibleItems;
            var guaranteedItems = lootPool.guaranteedItems;

            var results = new List<string>();
            
            if(guaranteedItems != null && guaranteedItems.Count > 0)
            {
                var itemList = new List<string>();

                foreach(var item in guaranteedItems)
                {
                    itemList.Add(item.item.ToString());
                }

                results.Add($"Open to receive {ArchString.StringList(itemList)}");
            }

            if(possibleItems != null && possibleItems.Count > 0)
            {
                results.Add($"Can contain {possibleItems.Count} more items");
            }

            return ArchString.NextLineList(results);
        }
    }
}
