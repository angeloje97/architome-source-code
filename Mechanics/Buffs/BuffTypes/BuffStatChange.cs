using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffStatChange : BuffType
    {
        // Start is called before the first frame update
        public Stats stats;
        public bool buffValueMultiplies;
        public float multiplier;
        public bool stacks;
        [SerializeField]Stats starting;
        new void GetDependencies()
        {
            base.GetDependencies();

            if (buffInfo)
            {
                buffInfo.OnBuffEnd += OnBuffEnd;

                if (buffValueMultiplies)
                {
                    stats *= buffInfo.properties.value * multiplier;
                }

                if (stacks)
                {

                    buffInfo.OnStack += OnStack;
                }

                ApplyBuff();
            }
            else
            {
                Destroy(gameObject);
            }

        }
        void Start()
        {
            GetDependencies();
        }

        void OnStack(BuffInfo info, int stacks, float value)
        {
            stats = starting*stacks;
            buffInfo.buffsManager.UpdateStats();
        }

        public void ApplyBuff()
        {
            starting = new();
            starting.Copy(stats);
            buffInfo.buffsManager.UpdateStats();
        }

        public void OnBuffEnd(BuffInfo buff)
        {
            stats.ZeroOut();
            buffInfo.buffsManager.UpdateStats();
        }
    }

}
