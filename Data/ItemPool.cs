using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class ItemPool : ScriptableObject
    {
        [System.Serializable]
        public class PossibleItem
        {
            public Item item;

            public int minAmount = 1;
            public int maxAmount = 100;
            public float chance;
        }

        public List<PossibleItem> possibleItems;

        public List<ItemData> CreatePossibleItems(int items, bool uniqueItemsOnly = false)
        {
            var itemDatas = new List<ItemData>();
            var possibleDatas = new List<ItemData>();

            foreach (var possibleItem in possibleItems)
            {
                var item = possibleItem.item;

                for (int i = 0; i < possibleItem.chance * 100; i++)
                {
                    int amount = Random.Range(1, possibleItem.maxAmount);
                    amount = Mathf.Clamp(amount, 1, item.maxStacks);

                    possibleDatas.Add(new() { item = item, amount = amount });
                }
            }

            var indeces = Enumerable.Range(0, possibleDatas.Count).ToList();

            for (int i = 0; i < items; i++)
            {

                if (indeces.Count == 0) break;

                var randomIndex = Random.Range(0, indeces.Count);

                var index = indeces[randomIndex];

                bool validItem = true;

                if (uniqueItemsOnly)
                {
                    foreach (var itemData in itemDatas) //Check if an item already exists.
                    {
                        if (itemData.item != possibleDatas[i].item) continue;

                        
                        validItem = false;
                        break;
                    }

                }

                if (!validItem)
                {
                    indeces.RemoveAt(randomIndex);
                    i--;
                    continue;
                }



                itemDatas.Add(new() { item = possibleDatas[index].item, amount = possibleDatas[index].amount });

                indeces.RemoveAt(randomIndex);
            }

            return itemDatas;
        }
    }
}
