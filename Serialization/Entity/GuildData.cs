using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public class GuildData
    {
        public EntityData.InventoryData inventory;
        public EntityData.InventoryData currencies;
        //public List<ItemData> inventory;
        //public List<ItemData> currencies;
        public int level;

        public GuildData(GuildManager guildManager)
        {
            var guildInfo = guildManager.guildInfo;
            inventory = new(guildInfo.vault);
            currencies = new(guildInfo.currencies);
            level = guildInfo.level;
        }

        public void LoadData(GuildManager.GuildInfo info)
        {
            info.level = level;
            info.vault = VaultInvetory();
            info.currencies = VaultCurrencies();
        }

        public List<ItemData> VaultInvetory()
        {
            var database = DataMap.active._maps;


            return inventory.ItemDatas(database);
        }

        public List<ItemData> VaultCurrencies()
        {
            var database = DataMap.active._maps;
            Debugger.UI(4333, $"Database != null ? {database != null}");
            var itemDatas = new List<ItemData>();
            if (currencies != null)
            {
                itemDatas = currencies.ItemDatas(database);
            }

            Debugger.UI(4332, $"{itemDatas.Count}");

            return itemDatas;
        }
    }
}
