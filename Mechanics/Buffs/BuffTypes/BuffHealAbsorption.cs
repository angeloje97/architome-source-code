using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffHealAbsorption : BuffType
    {


        private void Start()
        {
            GetDependencies();    
        }

        new void GetDependencies()
        {
            base.GetDependencies();

            var info = buffInfo.hostInfo;


            var unsubscribe = hostCombatEvent.AddListenerHealth(eHealthEvent.BeforeHealingTaken, BeforeHostHealingTaken, this);

            info.combatEvents.OnUpdateHealAbsorbShield += HandleUpdateAbsorbShield;
            


            info.UpdateHealthAbsorbShield();
            buffInfo.OnBuffEnd += (BuffInfo buff) => {
                unsubscribe();
                info.combatEvents.OnUpdateHealAbsorbShield -= HandleUpdateAbsorbShield;
                info.UpdateHealthAbsorbShield();
            };

        }

        void HandleUpdateAbsorbShield(EntityInfo entity, List<Func<float>> nums)
        {
            nums.Add(() => value);
        }

        void BeforeHostHealingTaken(HealthEvent eventData)
        {
            if (value >= eventData.value)
            {
                value -= eventData.value;
                eventData.SetValue(0f);
            }
            else if (value < eventData.value)
            {
                eventData.SetValue(eventData.value - value);
                value = 0f;
                buffInfo.Deplete();
            }

            eventData.target.UpdateHealthAbsorbShield();
        }

        public override string GeneralDescription()
        {
            return $"Prevents the unit from taking any healing by absorbing it for {valueContributionToBuffType * 100}% of the ability's value";
        }

        public override string Description()
        {

            return $"Prevents the unit from taking any healing for the next {value} health";
        }
    }
}
