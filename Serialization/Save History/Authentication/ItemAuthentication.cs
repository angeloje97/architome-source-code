using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architome
{
    public class ItemAuthentication : Authentication
    {
        public LogicType authenticationLogic;
        public List<ItemData> requiredItemsObtained;

        Dictionary<int, bool> values;

        public override void OnAuthenticationStart()
        {
            base.OnAuthenticationStart();
            UpdateValues();

            var validated = Validated();
            OnStartAuthentication?.Invoke(validated);
        }

        void UpdateValues()
        {
            values ??= new();
            var itemHistoryDatas = ItemHistory.active;

            if (itemHistoryDatas == null) return;
            
            foreach(var itemHistoryData in itemHistoryDatas)
            {
                UpdateValue(itemHistoryData.itemId, itemHistoryData.obtained);
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
            return new ArchLogic(valueList).Valid(authenticationLogic);
        }
    }
}
