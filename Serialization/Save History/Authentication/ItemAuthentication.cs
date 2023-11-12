using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class ItemAuthentication : Authentication
    {
        [Header("Item Authentication Fields")]
        HistoryRecorder recorder;
        ItemHistory historyDatas;
        public List<ItemData> requiredItemsObtained;
           

        Dictionary<int, bool> values;


        public override void OnAuthenticationStart()
        {
            base.OnAuthenticationStart();
            GetDependencies();
            CreateValues();

            Validated(fromStart:true);
        }

        void GetDependencies()
        {
            recorder = HistoryRecorder.active;
            historyDatas = ItemHistory.active;

            InvokerQueueHandler invokerHandler = new() { delayAmount = 5f, maxInvokesQueued = 2 };

            recorder.OnItemHistoryChange.AddListener(((EntityInfo, Inventory.LootEventData) data) => {
                invokerHandler.InvokeAction(() => {
                    var itemEvent = data.Item2;
                    var itemId = itemEvent.itemInfo.item._id;

                    if (!values.ContainsKey(itemId)) return;
                    if (values[itemId] == true) return;

                    UpdateValues();

                    Validated();
                });

            }, this);
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

        protected override void UpdateValues()
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

            ValidDictionary(values);
        }

        void UpdateValue(int id, bool obtained)
        {
            if (values.ContainsKey(id))
            {
                values[id] = obtained;
            }
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
