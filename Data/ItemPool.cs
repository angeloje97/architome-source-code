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

        public List<ItemData> GeneratedItems(List<PossibleItem> possibleItems, List<PossibleItem> guaranteedItems, int items)
        {
            var itemDatas = new List<ItemData>();

            HandleGuaranteedItems();
            HandlePossibleItems();

            return itemDatas;


            void HandleGuaranteedItems()
            {
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
                    if (chanceRole >= possible.chance) continue;

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
                if (itemDatas.Count >= items) return;

                var count = data.amount;

                while (count != 0)
                {
                    var item = Application.isPlaying ? Instantiate(data.item) : data.item;

                    itemDatas.Add(new() {
                        item = item,
                        amount = data.item.NewStacks(0, count, out count)
                    });
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
            return GeneratedItems(possibleItems, guaranteedItems, items);
        }

        public List<ItemData> ItemsFromRarity(int max, Rarity rarity)
        {

            var rarityItems = RarityItems(rarity);

            if (rarityItems != null)
            {
                return GeneratedItems(rarityItems.possibleItems, rarityItems.guaranteedItems, max);

            }
            else
            {
                return GeneratedItems(possibleItems, guaranteedItems, max);
            }


        }

        public List<ItemData> ItemsFromEntityRarity(int max, EntityRarity rarity)
        {

            var entityRarityItems = EntityItems(rarity);

            if (entityRarityItems != null)
            {
                return GeneratedItems(entityRarityItems.possibleItems, entityRarityItems.guaranteedItems, max);
            }
            else
            {
                return GeneratedItems(possibleItems, guaranteedItems, max);
            }

        }
    }
}
