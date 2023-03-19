using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class Shop : MonoBehaviour
    {
        [SerializeField] bool update;
        [Serializable]
        public class Info
        {
            public float buyMultiplier = 1f;
            public float sellMultiplier = .50f;
        }

        public Info info;

        public List<MerchData> merchandise;

        public Action<MerchData> OnMerchAction { get; set; }

        private void OnValidate()
        {
            if (!update) return;
            update = false;

        }

        private void Start()
        {
            UpdatePrices();
        }

        void UpdatePrices()
        {
            merchandise ??= new();
            foreach (var merch in merchandise)
            {
                merch.UpdatePrice(info.buyMultiplier);
            }
        }

        public bool Buy(MerchData merch, int amount, EntityInfo entity, out int leftOver)
        {
            leftOver = merch.amount;
            if (amount > merch.amount) return false;

            var totalPrice = amount * merch.price;

            if (!entity.CanPickUp(new(merch.item, amount))) return false;
            if (!entity.CanSpend(merch.currency, totalPrice)) return false;

            leftOver -= amount;
            return true;
        }
    }

    #region Merch Data

    [Serializable]
    public class MerchData : ItemData
    {
        public Currency currency;
        public int price;
        public int originalPrice;

        public MerchData(ItemData itemData, float priceMultiplier = 1f)
        {
            item = itemData.item;
            amount = itemData.amount;

            //price = Mathf.RoundToInt(amount * item.value * priceMultiplier);
        }

        public MerchData(ItemInfo itemInfo, float priceMultiplier = 1f) : base(itemInfo)
        {
            if (item == null)
            {
                price = 0;
                return;
            }
            originalPrice = Mathf.RoundToInt(amount * item.value * priceMultiplier);

        }

        public void UpdatePrice(float priceMultiplier = 1f)
        {
            if (item == null) return;
            price = (int) priceMultiplier * originalPrice;
        }
    }
    #endregion
}
