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
        public int level, gold;

        public GuildData(GuildManager guildManager)
        {
            var guildInfo = guildManager.guildInfo;
            SaveItems(guildInfo.vault);
            level = guildInfo.level;
            gold = guildInfo.gold;
        }
        
        public void SaveItems(List<ItemData> items)
        {
            inventory = new(items);
        }

        public void AddGold(int amount)
        {
            gold += amount;
        }

        public void LoadData(GuildManager.GuildInfo info)
        {
            info.level = level;
            info.gold = gold;
            info.vault = VaultInvetory();
        }

        public List<ItemData> VaultInvetory()
        {
            var database = DataMap.active._maps;


            return inventory.ItemDatas(database);
        }
    }
}
