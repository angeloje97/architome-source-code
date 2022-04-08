using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffStatChange : BuffType
    {
        // Start is called before the first frame update
        public Stats stats;

        new void GetDependencies()
        {
            base.GetDependencies();

            if (buffInfo)
            {
                buffInfo.OnBuffEnd += OnBuffEnd;
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

        // Update is called once per frame
        void Update()
        {

        }

        public void ApplyBuff()
        {
            buffInfo.buffsManager.UpdateStats();
        }

        public void OnBuffEnd(BuffInfo buff)
        {
            stats.ZeroOut();
            buffInfo.buffsManager.UpdateStats();
        }
    }

}
