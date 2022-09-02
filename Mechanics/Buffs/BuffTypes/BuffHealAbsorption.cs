using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffHealAbsorption : BuffType
    {

        [Header("Heal Absorption Properties")]
        public float absorptionValue;

        private void Start()
        {
            GetDependencies();    
        }

        new void GetDependencies()
        {
            base.GetDependencies();

            var info = buffInfo.sourceInfo;

            info.combatEvents.BeforeHealingTaken += BeforeHostHealingTaken;

            buffInfo.OnBuffEnd += (BuffInfo buff) => {
                info.combatEvents.BeforeHealingTaken -= BeforeHostHealingTaken;
            };

            absorptionValue = value;
        }

        void BeforeHostHealingTaken(CombatEventData eventData)
        {
            if (absorptionValue > eventData.value)
            {
                absorptionValue -= eventData.value;
                eventData.value = 0f;
                return;
            }
            else if (absorptionValue < eventData.value)
            {
                eventData.value -= absorptionValue;
                absorptionValue = 0f;
            }

            buffInfo.Cleanse();
        }

        public override string GeneralDescription()
        {
            return $"Prevents the unit from taking any healing by absorbing it for {valueContributionToBuffType * 100}% of the ability's value";
        }

        public override string Description()
        {
            var value = absorptionValue > 0f ? absorptionValue : this.value;

            return $"Prevents the unit from taking any healing for the next {value} health";
        }
    }
}
