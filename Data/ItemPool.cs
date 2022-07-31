using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    public class ItemPool : ScriptableObject
    {

        [System.Serializable]
        public class EntityRarityPool
        {
            public string name
            {
                get
                {
                    return rarity.ToString();
                }
            }

            public EntityRarity rarity;
            public List<PossibleItem> possibleItems;
            public List<PossibleItem> guaranteedItems;
        }

        [System.Serializable]
        public class RarityPool
        {
            public string name
            {
                get
                {
                    return rarity.ToString();
                }
            }

            public Rarity rarity;
            public List<PossibleItem> possibleItems;
            public List<PossibleItem> guaranteedItems;
        }

        [System.Serializable]
        public class PossibleItem
        {
            public Item item;

            public int minAmount = 1;
            public int maxAmount = 100;
            [Range(0f, 100f)]
            public float chance;
        }

        public List<EntityRarityPool> entityItems;
        public List<RarityPool> rarityItems;

        public List<PossibleItem> possibleItems;
        public List<PossibleItem> guaranteedItems;

        EntityRarityPool EntityItems(EntityRarity rarity)
        {
            foreach (var entityItem in entityItems)
            {
                if (entityItem.rarity == rarity)
                {
                    return entityItem;
                }
            }

            return null;
        }

        RarityPool RarityItems(Rarity rarity)
        {
            foreach (var rarityItem in rarityItems)
            {
                if (rarity == rarityItem.rarity)
                {
                    return rarityItem;
                }
            }

            return null;
        }

        public List<ItemData> GeneratedItems(RequestData request)
        {
            var itemDatas = new List<ItemData>();

            var guaranteedItems = request.guaranteedItems;
            var possibleItems = request.possibleItems;

            if (possibleItems == null && request.replaceNull)
            {
                possibleItems = this.possibleItems;
            }

            if (guaranteedItems == null && request.replaceNull)
            {
                guaranteedItems = this.possibleItems;
            }



            var targetCount = Random.Range(request.minItems, request.maxItems);

            if (request.minItems > request.maxItems)
            {
                request.minItems = request.maxItems;
                targetCount = request.minItems;
            }

            HandleGuaranteedItems();


            if (request.useMinMax)
            {
                while (itemDatas.Count < targetCount)
                {
                    if (possibleItems.Count == 0) break;
                    HandlePossibleItems();
                }
            }
            else
            {
                HandlePossibleItems();
            }
            

            return itemDatas;


            void HandleGuaranteedItems()
            {
                if (guaranteedItems == null) return;
                foreach (var guaranteed in guaranteedItems)
                {
                    var randomAmount = Random.Range(guaranteed.minAmount, guaranteed.maxAmount);
                    AddItem(new()
                    {
                        item = guaranteed.item,
                        amount = randomAmount,
                    });
                }
            }

            void HandlePossibleItems()
            {
                foreach (var possible in possibleItems)
                {
                    var chanceRole = Random.Range(0f, 100f);
                    var chance = possible.chance;

                    if (request.chanceMultiplier > 0f)
                    {
                        chance *= request.chanceMultiplier;
                    }

                    if (chanceRole >= chance) continue;

                    var randomAmount = Random.Range(possible.minAmount, possible.maxAmount);

                    AddItem(new()
                    {
                        item = possible.item,
                        amount = randomAmount,
                    });
                }
            }

            void AddItem(ItemData data)
            {
                if (itemDatas.Count >= targetCount) return;

                var count = data.amount;

                while (count != 0)
                {
                    var item = Application.isPlaying ? Instantiate(data.item) : data.item;

                    itemDatas.Add(new() {
                        item = item,
                        amount = data.item.NewStacks(0, count, out count)
                    });

                    if (itemDatas.Count >= targetCount) break;
                    //if (itemDatas.Count >= items) return;
                    //if (data.item.count >= data.item.maxStacks )
                    //{
                    //    itemDatas.Add(new() { item = data.item, amount = data.item.maxStacks });
                    //    count -= data.item.maxStacks;
                    //}
                    //else
                    //{
                    //    itemDatas.Add(new() { item = data.item, amount = count });
                    //    count = 0;
                    //    break;
                    //}
                }

            }
        }

        public List<ItemData> CreatePossibleItems(int items)
        {
            return GeneratedItems(new()
            {
                maxItems = items,
                possibleItems = possibleItems,
                guaranteedItems = guaranteedItems
            });
        }

        public List<ItemData> ItemsFromRarity(Rarity rarity, RequestData data)
        {

            var rarityItems = RarityItems(rarity);

            if (rarityItems != null)
            {
                data.possibleItems = rarityItems.possibleItems;
                data.guaranteedItems = rarityItems.guaranteedItems;
                data.chanceMultiplier = 0f;
                return GeneratedItems(data);

            }
            else
            {
                return GeneratedItems(data);
            }


        }

        public List<ItemData> ItemsFromEntityRarity(EntityRarity rarity, RequestData request)
        {

            var entityRarityItems = EntityItems(rarity);

            if (entityRarityItems != null)
            {
                request.possibleItems = entityRarityItems.possibleItems;
                request.guaranteedItems = entityRarityItems.guaranteedItems;
                request.chanceMultiplier = 0f;
                
                return GeneratedItems(request);
            }
            else
            {
                return GeneratedItems(request);
            }

        }

        public class RequestData
        {
            public int maxItems;
            public int minItems;
            public float chanceMultiplier = 0f;
            public bool useMinMax;
            public bool replaceNull;
            public List<PossibleItem> possibleItems;
            public List<PossibleItem> guaranteedItems;
        }
    }
}
