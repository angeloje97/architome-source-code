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

            var buffsToCleanse = buffInfo.buffsManager.Buffs()
                                .Where(buffInfo => buffInfo.GetComponent<BuffStateChanger>() != null &&
                                statesImmuneTo.Contains(buffInfo.GetComponent<BuffStateChanger>().stateToChange)).ToList();

            foreach (var buff in buffsToCleanse)
            {
                buff.Cleanse();
            }

            
        }
        void Start()
        {
            GetDependencies();
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
