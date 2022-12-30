using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class BuffEntityStateImmune : BuffType
{
        // Start is called before the first frame update

        public List<EntityState> statesImmuneTo;
        public bool triggerEvents;

        new void GetDependencies()
        {
            base.GetDependencies();

            buffInfo.OnBuffEnd += OnBuffEnd;

            ApplyBuff();
        }

        void ApplyBuff()
        {

            for(int i = 0; i < statesImmuneTo.Count; i++)
            {
                var state = statesImmuneTo[i];
                buffInfo.hostInfo.stateImmunities.Add(state);

                if (triggerEvents)
                {
                    ArchAction.Delay(() => {
                        buffInfo.hostInfo.combatEvents.OnAddImmuneState?.Invoke(state);
                    }, i * .25f);

                }
            }

            buffInfo.buffsManager.CleanseImmunity();

            
        }

        public override string Description()
        {
            var result = "";

            if (statesImmuneTo == null || statesImmuneTo.Count == 0)
            {
                return result;
            }

            result += $"Cannot be effected by: ";

            var list = new List<string>();

            foreach (var state in statesImmuneTo)
            {
                list.Add(state.ToString());
            }

            var listString = ArchString.StringList(list);

            result += $"{listString}.";

            return result;
        }

        public override string GeneralDescription()
        {
            return Description();
        }

        private void Awake()
        {
            GetDependencies();
        }
        void Start()
        {
            //GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnBuffEnd(BuffInfo buff)
        {
            var removeDict = new Dictionary<EntityState, bool>();

            foreach(var immunity in statesImmuneTo)
            {
                if (removeDict.ContainsKey(immunity)) continue;
                removeDict.Add(immunity, false);
            }

            for(int i = 0; i < buffInfo.hostInfo.stateImmunities.Count; i++)
            {
                var state = buffInfo.hostInfo.stateImmunities[i];

                if (!removeDict.ContainsKey(state)) continue;
                if (removeDict[state]) continue;
                removeDict[state] = true;

                if (triggerEvents)
                {
                    ArchAction.Delay(() => {
                        buffInfo.hostInfo.combatEvents.OnRemoveImmuneState?.Invoke(state);
                    }, i * .25f);
                }

                buffInfo.hostInfo.stateImmunities.RemoveAt(i);
                i--;
            }
            //for (int i = 0; i < buffInfo.hostInfo.stateImmunities.Count; i++)
            //{
            //    if (statesImmuneTo.Contains(buffInfo.hostInfo.stateImmunities[i]))
            //    {
            //        statesImmuneTo.Remove(buffInfo.hostInfo.stateImmunities[i]);
            //        buffInfo.hostInfo.stateImmunities.RemoveAt(i);
            //        i--;
            //    }
            //}
        }
    }

}
