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

            //foreach (var buff in buffInfo.buffsManager.Buffs())
            //{
            //    if (!buff.GetComponent<BuffStateChanger>()) continue;
            //}

            //var buffsToCleanse = buffInfo.buffsManager.Buffs()
            //                    .Where(buffInfo => buffInfo.GetComponent<BuffStateChanger>() != null &&
            //                    statesImmuneTo.Contains(buffInfo.GetComponent<BuffStateChanger>().stateToChange)).ToList();

            //foreach (var buff in buffsToCleanse)
            //{
            //    buff.Cleanse();
            //}

            
        }

        public override string Description()
        {
            var result = "";

            if (statesImmuneTo == null || statesImmuneTo.Count == 0)
            {
                return result;
            }

            result += $"Target is immune to";

            var list = new List<string>();

            foreach (var state in statesImmuneTo)
            {
                list.Add(state.ToString());
            }

            var listString = ArchString.StringList(list);

            result += $"{listString}\n";

            return result;
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
