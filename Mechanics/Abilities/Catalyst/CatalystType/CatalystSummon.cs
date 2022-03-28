using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystSummon : CatalystProp
    {
        // Start is called before the first frame update
        public AbilityInfo.SummoningProperty summoning;

        new void GetDependencies()
        {
            base.GetDependencies();

            if (ability)
            {
                summoning = ability.summoning;
            }

            if (catalyst)
            {
                catalyst.OnCatalystDestroy += OnCatalystDestroy;
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

        public void OnCatalystDestroy(CatalystDeathCondition deathCondition)
        {

        }
    }

}
