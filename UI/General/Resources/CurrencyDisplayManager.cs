using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class CurrencyDisplayManager : MonoBehaviour
    {
        GuildManager guildManager;
        Dictionary<int, CurrencyDisplay> idDisplayMap;

        [Header("Properties")]
        public List<CurrencyDisplay> displays;

        [Header("Prefabs")]
        public CurrencyDisplay displayPrefab;


        List<ItemData> currencyDatas;

        void Start()
        {
            GetDependencies();
            HandleStart();
            HandleNullGuildManager();
        }
        void GetDependencies()
        {
            guildManager = GuildManager.active;

            if (guildManager)
            {
                guildManager.OnCurrenciesChange += HandleCurrencyChange;
            }
        }
        void HandleStart()
        {
            if (displayPrefab == null) return;
            if (guildManager == null) return;

            idDisplayMap = new();
            displays = new();
            
            foreach (var itemData in guildManager.guildInfo.currencies)
            {
                UpdateDisplay(itemData);
            }
        }
        void HandleCurrencyChange(List<ItemData> currencies)
        {
            foreach (var itemData in currencies)
            {
                UpdateDisplay(itemData);
            }
        }
        CurrencyDisplay UpdateDisplay(ItemData itemData)
        {
            idDisplayMap ??= new();
            displays ??= new();

            if (!itemData.item.IsCurrency()) return null;

            var currency = (Currency) itemData.item;
            if (idDisplayMap.ContainsKey(currency._id))
            {

                var currentDisplay = idDisplayMap[currency._id];
                currentDisplay.UpdateCurrencyDisplay(itemData.amount);
                return currentDisplay;
            }

            var newDisplay = Instantiate(displayPrefab.gameObject, transform).GetComponent<CurrencyDisplay>();

            newDisplay.SetCurrencyDisplay(currency, itemData.amount);

            displays.Add(newDisplay);
            idDisplayMap.Add(currency._id, newDisplay);

            return newDisplay;
        }
        async void HandleNullGuildManager()
        {
            if (guildManager != null) return;
            var currentSave = Core.currentSave;
            if (currentSave == null) return;

            var dataMap = DataMap.active;

            while (dataMap == null)
            {
                dataMap = DataMap.active;
                await Task.Yield();
            }


            currencyDatas = currentSave.guildData.currencies.ItemDatas(dataMap._maps);

            var gameManager = GameManager.active;

            gameManager.OnNewPlayableEntity += delegate (EntityInfo entity, int index)
            {
                entity.infoEvents.OnCanSpendCheck += delegate (Currency currency, int amount, List<bool> checks)
                {
                    checks.Add(SpendCurrency(currency, amount));
                };
            };

            SaveSystem.active.BeforeSave += delegate (SaveGame save)  {
                save.guildData.currencies = new(currencyDatas);
            };
            


            foreach (var currencyData in currencyDatas)
            {
                UpdateDisplay(currencyData);
            }
        }
        bool SpendCurrency(Currency currency, int amount)
        {
            if (currencyDatas == null) return false;

            foreach (var currencyData in currencyDatas)
            {
                if (!currencyData.item.Equals(currency)) continue;
                if (currencyData.amount < amount) continue;

                currencyData.amount -= amount;

                if (currencyData.amount <= 0)
                {
                    currencyDatas.Remove(currencyData);
                }

                return true;
            }
            return false;
        }
    }
}
