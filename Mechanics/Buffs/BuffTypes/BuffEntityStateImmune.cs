using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

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

            foreach (var buff in transform.parent.GetComponentsInChildren<BuffStateChanger>())
            {
                var buffInfo = buff.GetComponent<BuffInfo>();
                if (buffInfo.buffTargetType == BuffTargetType.Harm)
                {
                    buffInfo.Cleanse();
                }
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
