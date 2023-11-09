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
            CreateValues();
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
        
        void CreateValues()
        {
            if (requiredItemsObtained == null) return;
            values = new();

            foreach(var data in requiredItemsObtained)
            {
                values.Add(data.item._id, false);
            }
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
            if (recorder == null) return;

            var recorderData = recorder.itemHistoryDatas;

            foreach(var data in requiredItemsObtained)
            {
                var id = data.item._id;
                var obtainedFromHistory = historyDatas.HasPickedUp(id);
                var obtainedFromRecorder = recorderData.ContainsKey(id) && recorderData[id].obtained;

                if(obtainedFromHistory || obtainedFromRecorder)
                {
                    UpdateValue(id, true);
                }
            }
        }

        void UpdateValue(int id, bool obtained)
        {
            if (values.ContainsKey(id))
            {
                values[id] = obtained;
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

        public override AuthenticationDetails Details()
        {
            var details = new AuthenticationDetails();

            UpdateValues();

            
            foreach(var data in requiredItemsObtained)
            {
                var id = data.item._id;

                var list = values[id] ? details.validValues : details.invalidValues;

                list.Add(data.item.ToString());
            }

            return details;
        }
    }
}
