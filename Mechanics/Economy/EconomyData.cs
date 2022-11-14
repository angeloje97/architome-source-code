using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Economy", menuName = "Architome/Economy/New Economy")]
    public class EconomyData : ScriptableObject
    {
        [SerializeField] bool updateNames;
        [SerializeField] bool updateFamilyOrder;

        [Serializable]
        public class Stock
        {
            [HideInInspector] public string name;
            public Currency currency;
            public float value = 1f;
        }

        [Serializable]
        public class StockFamily
        {
            public string name;
            public List<Currency> currencies;
            public bool isPrime;

        }

        public List<StockFamily> stockFamilies = new();
        public List<Stock> stocks = new();

        public StockFamily PrimeFamily()
        {
            foreach(var family in stockFamilies)
            {
                if (family.isPrime)
                {
                    return family;
                }
            }
            return null;
        }

        private void OnValidate()
        {

            UpdateNames();
            UpdateFamilyOrder();

            void UpdateNames()
            {
                if (!updateNames) return;
                updateNames = false;

                foreach(var stock in stocks)
                {
                    stock.name = $"{stock.currency} ({stock.value}g)";
                }
            }

            void UpdateFamilyOrder()
            {
                if (!updateFamilyOrder) return;
                updateFamilyOrder = false;

                foreach(var family in stockFamilies)
                {
                    family.currencies = family.currencies.OrderBy(currency => currency.value).ToList();
                }
            }
        }

    }
}
