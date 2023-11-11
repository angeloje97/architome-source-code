using Architome.History;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Architome
{
    public class EntityKilledAuthentication : Authentication
    {
        public List<EntityInfo> validEntities;
        HistoryRecorder recorder;
        EntityHistory entityHistory;
        Dictionary<int, bool> values;

        public override void OnAuthenticationStart()
        {
            base.OnAuthenticationStart();
            HandleRecorder();
            InitiateValues();
            UpdateValues();

            var validated = Validated();

            OnStartAuthentication?.Invoke(validated);
        }

        void InitiateValues()
        {
            values = new();

            foreach(var entity in validEntities)
            {
                var id = entity._id;
                values.Add(id, false);
            }
        }

        void HandleRecorder()
        {
            recorder = HistoryRecorder.active;
            if (recorder == null) return;

            InvokerQueueHandler queueHandler = new() { delayAmount = 5f, maxInvokesQueued = 2 };

            recorder.OnEntitySlainHistoryChange.AddListener(((EntityInfo, int) tuple) => {

                queueHandler.InvokeAction(() => {
                    var entity = tuple.Item1;
                    var kills = tuple.Item2;
                    var id = entity._id;
                    if (!values.ContainsKey(id)) return;
                    var killedBefore = kills > 0;
                    if (values[id] == killedBefore) return;

                    var current = authenticated;
                    UpdateValues();
                    if(current != authenticated)
                    {
                        OnAuthenticationChange?.Invoke(authenticated);
                    }

                });


            }, this);
        }


        void UpdateValues()
        {
            var currentEnemiesKilled = recorder.currentEnemyKills;
                
            foreach(var entity in validEntities)
            {
                var id = entity._id;
                var historyKillCount = entityHistory.KillCount(id);
                int recorderKills = currentEnemiesKilled.ContainsKey(id) ? currentEnemiesKilled[id] : 0;
                var valid = historyKillCount > 0 || recorderKills > 0;

                UpdateValue(entity, valid);

            }
        }

        void UpdateValue(EntityInfo target, bool killed)
        {
            var id = target._id;

            if (values.ContainsKey(id))
            {
                values[id] = killed;
            }
        }

        public override bool Validated(bool updateValues = false)
        {
            if (updateValues) UpdateValues();
            return ValidDictionary(values);
        }
    }
}
