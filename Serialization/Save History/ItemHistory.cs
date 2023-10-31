using System;
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

        public bool HasPickedUp(Item item)
        {
            var id = item._id;

            if (!itemHashes.ContainsKey(id))
            {
                itemHashes.Add(id, new(item));
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
    }
}
