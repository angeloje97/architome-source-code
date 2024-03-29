﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architome
{
    [Serializable]
    public class ItemHistory
    {
        //Singleton;
        public static ItemHistory active;

        Dictionary<int, ItemHistoryData> itemHashes;


        public void SetActiveSingleTon()
        {
            itemHashes ??= new();

            active = this;
        }

        public void AddItemPickedUp(ItemInfo item)
        {
            var itemId = item.item._id;
            ItemHistoryData currentData;
            if (itemHashes.ContainsKey(itemId))
            {
                currentData = itemHashes[itemId];
            }
            else
            {
                currentData = new(item.item);
                itemHashes.Add(itemId, currentData);                
            }
            currentData.obtained = true;
        }

        public IEnumerator<ItemHistoryData> GetEnumerator()
        {
            itemHashes ??= new();

            foreach(KeyValuePair<int, ItemHistoryData> key in itemHashes)
            {
                yield return key.Value;
            }
        }

        public void UpdateHistoryData(ItemHistoryData data)
        {
            var id = data.itemId;

            if (!itemHashes.ContainsKey(id))
            {
                itemHashes.Add(id, data);
            }
            else
            {
                if (data.obtained)
                {
                    itemHashes[id].obtained = true;
                }
            }
        }

        public bool HasPickedUp(int id)
        {

            if (!itemHashes.ContainsKey(id))
            {
                itemHashes.Add(id, new(id));
            }

            return itemHashes[id].obtained;
        }
    }

    [Serializable]
    public class ItemHistoryData 
    {
        public int itemId;

        public bool obtained;

        public ItemHistoryData(Item data)
        {
            itemId = data._id;
        }

        public ItemHistoryData(int itemId, bool obtained = false)
        {
            this.itemId = itemId;
            this.obtained = obtained;
        }
    }
}
