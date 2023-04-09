using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public class CoreLoader
    {
        #region Items
        public static void LoadInventory(EntityData.InventoryData inventoryData, List<InventorySlot> slots)
        {
            var world = World.active;
            var worldActions = WorldActions.active;
            var dataMaps = DataMap.active;

            if (world == null || worldActions == null || dataMaps == null) return;
            if (inventoryData == null || slots == null) return;

            var itemTemplate = world.prefabsUI.item;
            var itemDatas = inventoryData.ItemDatas(dataMaps._maps);

            for(int i = 0; i < itemDatas.Count; i++)
            {
                if (i >= slots.Count) break;
                if (itemDatas[i].item == null) continue;

                worldActions.CreateItemUI(itemDatas[i], slots[i], itemTemplate);
            }

        }
        #endregion
    }
}
