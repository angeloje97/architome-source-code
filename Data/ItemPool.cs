using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Item Pool", menuName = "Architome/Item/Item Pool")]
    public class ItemPool : ScriptableObject
    {
        [SerializeField] bool update;

        #region Serializables

        [System.Serializable]
        public class PoolData
        {
            public string name;
            public List<PossibleItem> possibleItems, guaranteedItems;
        }

        [System.Serializable]
        public class EntityRarityPool: PoolData
        {
            public EntityRarity rarity;
        }

        [System.Serializable]
        public class RarityPool: PoolData
        {

            public Rarity rarity;
        }

        [System.Serializable]
        public class PossibleItem
        {
            public string name;
            public Item item;

            public int minAmount = 1;
            public int maxAmount = 100;
            [Range(0f, 100f)]
            public float chance;

            public void OnValidate()
            {

            }
        }

        [System.Serializable]
        public class RequestData
        {
            public int maxItems;
            public int minItems;
            public float chanceMultiplier = 0f;
            public bool useMinMax;
            public bool replaceNull;
            public bool uniqueItems;
            public List<PossibleItem> possibleItems;
            public List<PossibleItem> guaranteedItems;


            public void CleanSelf()
            {
                possibleItems = null;
                guaranteedItems = null;
            }
        }

        #endregion

        public List<EntityRarityPool> entityItems;
        public List<RarityPool> rarityItems;
        public List<PoolData> genericItemPools;

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
            rarityItems ??= new();

            foreach (var rarityItem in rarityItems)
            {
                if (rarity == rarityItem.rarity)
                {
                    return rarityItem;
                }
            }

            return null;
        }

        public PoolData ItemsFromName(string name)
        {
            foreach(var pooldata in genericItemPools)
            {
                if(pooldata.name.Equals(name))
                {
                    return pooldata;
                }
            }

            return null;
        }

        public List<ItemData> GeneratedItems(RequestData request)
        {
            var itemDatas = new List<ItemData>();

            var guaranteedItems = request.guaranteedItems;
            var possibleItems = request.possibleItems;
            var itemIds = new List<int>();

            if (possibleItems == null && request.replaceNull)
            {
                possibleItems = this.possibleItems;
            }

            if (guaranteedItems == null && request.replaceNull)
            {
                guaranteedItems = this.guaranteedItems;
            }



            var targetCount = Random.Range(request.minItems, request.maxItems);

            if (request.minItems > request.maxItems)
            {
                request.minItems = request.maxItems;
                targetCount = request.minItems;
            }

            HandleGuaranteedItems();

            int tries = 0;
            if (request.useMinMax)
            {
                while (itemDatas.Count < targetCount)
                {
                    if (possibleItems.Count == 0) break;
                    HandlePossibleItems();
                    tries++;
                    if (tries > 1000)
                    {
                        break;
                    }
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
                if (possibleItems == null) return;


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
                if (itemDatas.Count >= targetCount && request.useMinMax) return;

                var count = data.amount;
                if (!IsUnique(data)) return;

                while (count != 0)
                {
                    var item = Application.isPlaying ? Instantiate(data.item) : data.item;

                    itemDatas.Add(new() {
                        item = item,
                        amount = data.item.NewStacks(0, count, out count)
                    });

                    if (itemDatas.Count >= targetCount) break;
                }

            }

            bool IsUnique(ItemData itemData)
            {
                if (!request.uniqueItems) return true;
                var id = itemData.item._id;
                if (itemIds.Contains(id))
                {
                    return false;
                }

                itemIds.Add(id);

                return true;
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


            return ItemsFromPoolData(RarityItems(rarity), data);


        }

        public List<ItemData> ItemsFromEntityRarity(EntityRarity rarity, RequestData request)
        {

            return ItemsFromPoolData(EntityItems(rarity), request);

        }
        public List<ItemData> ItemsFromCustom(string customName, RequestData request)
        {
            return ItemsFromPoolData(ItemsFromName(customName), request);
        }

        List<ItemData> ItemsFromPoolData(PoolData poolData, RequestData request)
        {
            if(poolData != null)
            {
                request.possibleItems = poolData.possibleItems;
                request.guaranteedItems = poolData.guaranteedItems;
                Debugger.UI(5401, $"Guaranteed Items: {request.guaranteedItems.Count}");
                request.chanceMultiplier = 0;

                return GeneratedItems(request);
            }
            else
            {
                return GeneratedItems(request);
            }
        }

        



        #region Validate

        private void OnValidate()
        {
            if (!update) return;
            update = false;

            UpdatePossibleItems(possibleItems);
            UpdatePossibleItems(guaranteedItems);
            UpdateEntityItems(entityItems);
            UpdateRarityItems(rarityItems);
            UpdateGeneric();
        }

        public void UpdatePossibleItems(List<PossibleItem> possibleItems)
        {
            if (possibleItems == null) return;
            foreach(var item in possibleItems)
            {
                var range = item.minAmount == item.maxAmount ? $"({item.minAmount})" : $"({item.minAmount}-{item.maxAmount})";
                item.name = $"{item.item} {range} ({item.chance}%)";
            }
        }

        public void UpdateEntityItems(List<EntityRarityPool> rarityPool)
        {
            if (rarityPool == null) return;

            foreach(var rarity in rarityPool)
            {
                rarity.name = rarity.rarity.ToString();

                UpdatePossibleItems(rarity.possibleItems);
                UpdatePossibleItems(rarity.guaranteedItems);
            }
        }

        public void UpdateRarityItems(List<RarityPool> rarityPool)
        {
            if (rarityPool == null) return;

            foreach(var pool in rarityPool)
            {
                pool.name = pool.rarity.ToString();

                UpdatePossibleItems(pool.possibleItems);
                UpdatePossibleItems(pool.guaranteedItems);
            }
        }

        public void UpdateGeneric()
        {
            foreach(var generic in genericItemPools)
            {
                UpdatePossibleItems(generic.possibleItems);
                UpdatePossibleItems(generic.guaranteedItems);
            }
        }

        #endregion

    }
}
