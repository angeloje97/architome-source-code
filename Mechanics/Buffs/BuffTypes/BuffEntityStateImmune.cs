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

        new void GetDependencies()
        {
            base.GetDependencies();

            buffInfo.OnBuffEnd += OnBuffEnd;

            ApplyBuff();
        }

        void ApplyBuff()
        {
            foreach (var state in statesImmuneTo)
            {
                buffInfo.hostInfo.stateImmunities.Add(state);
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

            result += $"Cannot be effected by";

            var list = new List<string>();

            foreach (var state in statesImmuneTo)
            {
                list.Add(state.ToString());
            }

            var listString = ArchString.StringList(list);

            result += $"{listString}\n";

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
            for (int i = 0; i < buffInfo.hostInfo.stateImmunities.Count; i++)
            {
                if (statesImmuneTo.Contains(buffInfo.hostInfo.stateImmunities[i]))
                {
                    statesImmuneTo.Remove(buffInfo.hostInfo.stateImmunities[i]);
                    buffInfo.hostInfo.stateImmunities.RemoveAt(i);
                    i--;
                }
            }
        }
    }

}
