using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class Economy : MonoBehaviour
    {
        public static Economy active;
        public EconomyData data;

        public Dictionary<int, EconomyData.Stock> currencyMap;
        public Dictionary<int, EconomyData.StockFamily> currencyFamilyMap;

        void Start()
        {
            
        }

        private void Awake()
        {
            active = this;
            UpdateCurrencyMap();
        }

        public EconomyData.Stock Stock(Currency currency)
        {
            if (!currencyMap.ContainsKey(currency._id)) return null;
            return currencyMap[currency._id];
        }

        public EconomyData.StockFamily StockFamily(Currency currency)
        {
            if (!currencyFamilyMap.ContainsKey(currency._id)) return null;

            return currencyFamilyMap[currency._id];
        }

        void UpdateCurrencyMap()
        {
            if (data == null) return;
            currencyMap = new();
            currencyFamilyMap = new();

            foreach(var stock in data.stocks)
            {
                currencyMap.Add(stock.currency._id, stock);
            }

            foreach(var family in data.stockFamilies)
            {
                foreach (var currency in family.currencies)
                {
                    currencyFamilyMap.Add(currency._id, family);
                }
            }
        }

        public float ConvertCurrency(Currency from, Currency to)
        {
            var fromStock = Stock(from);
            var toStock = Stock(to);

            return fromStock.value / toStock.value;
        }

        public ConversionData AutoConvert(ItemData itemData)
        {
            var conversionData = new ConversionData();

            if (!itemData.item.IsCurrency()) return conversionData;
            var currency = (Currency) itemData.item;
            var stockFamily = StockFamily(currency);
            if (stockFamily == null) return conversionData;

            foreach(var item in stockFamily.currencies)
            {
                if (item.value < itemData.item.value) continue;
                if (item.Equals(itemData.item)) continue;

                var conversionRate = ConvertCurrency(currency, item);
                var inverseValue = 1 / conversionRate;
                var value = conversionRate * itemData.amount;

                if (value < 1) break;

                var floorValue = Mathf.Floor(value);
                var leftOverPercent = value - floorValue;
                var leftOver = inverseValue * leftOverPercent;

                conversionData.successful = true;
                conversionData.leftOver = (int) leftOver;
                conversionData.convertedItemData = new() { item = item, amount = (int) floorValue };

                break;

            }

            return conversionData;
        }

        public List<ItemData> PrimeValues(float value)
        {
            var result = new List<ItemData>();
            var primeFamily = data.PrimeFamily();
            if (primeFamily == null) return result;

            for (int i = primeFamily.currencies.Count - 1; i >= 0; i--)
            {
                var currency = primeFamily.currencies[i];
                var currencyValue = currency.value;
                if (value < currencyValue) continue;

                var ratio = value / currencyValue;

                var amount = Mathf.Floor(ratio);

                var remainderPercent = ratio - amount;

                result.Insert(0, new() {
                    item = currency,
                    amount = (int) amount
                });

                value *= remainderPercent;

            }

            return result;
            
        }

        public class ConversionData
        {
            public bool successful = false;
            public int leftOver;
            public ItemData convertedItemData;

        }
    }
}
