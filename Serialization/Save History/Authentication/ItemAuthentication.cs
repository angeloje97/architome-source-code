using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architome
{
    public class ItemAuthentication : Authentication
    {

        HistoryRecorder recorder;
        ItemHistory historyDatas;
        public LogicType authenticationLogic;
        public List<ItemData> requiredItemsObtained;
           

        Dictionary<int, bool> values;

        bool validated;

        public override void OnAuthenticationStart()
        {
            base.OnAuthenticationStart();
            GetDependencies();
            UpdateValues();

            var validated = Validated();
            OnStartAuthentication?.Invoke(validated);
        }

        void GetDependencies()
        {
            recorder = HistoryRecorder.active;
            historyDatas = ItemHistory.active;

            recorder.OnItemHistoryChange.AddListener(HandleChange, this);
        }

        void HandleChange((EntityInfo, Inventory.LootEventData) data)
        {
            UpdateValues();

            bool current = validated;
            validated = Validated();

            if(current != validated)
            {
                OnAuthenticationChange?.Invoke(validated);
            }
        }

        void UpdateValues()
        {
            values ??= new();

            if (historyDatas == null) return;
            
            foreach(var itemHistoryData in historyDatas)
            {
                UpdateValue(itemHistoryData.itemId, itemHistoryData.obtained);
            }

            foreach (KeyValuePair<int, ItemHistoryData> pair in recorder.itemHistoryDatas)
            {
                UpdateValue(pair.Key, pair.Value.obtained);
            }
        }

        void UpdateValue(int id, bool obtained)
        {
            if (values.ContainsKey(id))
            {
                values[id] = obtained;
            }
            else
            {
                values.Add(id, obtained);
            }
        }

        public override bool Validated(bool updateValues = false)
        {
            if (updateValues) UpdateValues();

            var valueList = values
                .Select((KeyValuePair<int, bool> pairs) => pairs.Value)
                .ToList();

            validated = new ArchLogic(valueList).Valid(authenticationLogic);
            return validated;
        }

    }
}
