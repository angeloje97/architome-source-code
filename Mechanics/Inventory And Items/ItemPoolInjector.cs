using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    public class ItemPoolInjector : MonoBehaviour
    {
        public UnityEvent<List<ItemData>> OnGenItems;
        public UnityEvent<List<ItemData>> OnGenItemsRarity;
        public UnityEvent<List<ItemData>> OnGenEntityRarity;
        public ItemPool itemPool;

        [SerializeField] bool testGenerate;
        [SerializeField] Rarity rarity;
        [SerializeField] EntityRarity entityRarity;
        [SerializeField] bool generateOnStart;


        [SerializeField] ItemPool.RequestData requestData;
        void Start()
        {
            if (generateOnStart)
            {
                GenerateItems();
                GenerateItemsFromRarity(rarity);
                GenerateItemsFromEntityRarity(entityRarity);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnValidate()
        {
            if (testGenerate)
            {
                testGenerate = false;
                GenerateItems();
                GenerateItemsFromRarity(rarity);
                GenerateItemsFromEntityRarity(entityRarity);
            }
        }


        public void GenerateItems()
        {
            if (itemPool == null) return;
            requestData.CleanSelf();

            var generatedItems = itemPool.GeneratedItems(requestData);

            OnGenItems?.Invoke(generatedItems);
        }

        public void GenerateItemsFromRarity(Rarity rarity)
        {
            if (itemPool == null) return;
            requestData.CleanSelf();

            var generatedItems = itemPool.ItemsFromRarity(rarity, requestData);
            OnGenItemsRarity?.Invoke(generatedItems);
        }

        public void GenerateItemsFromEntityRarity(EntityRarity entityRarity)
        {
            if (itemPool == null) return;
            requestData.CleanSelf();

            var generatedItems = itemPool.ItemsFromEntityRarity(entityRarity, requestData);
            OnGenEntityRarity?.Invoke(generatedItems);
        }
    }
}
