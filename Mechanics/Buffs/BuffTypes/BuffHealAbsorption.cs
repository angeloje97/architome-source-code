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


            var unsubscribe = hostCombatEvents.AddListenerHealth(eHealthEvent.BeforeHealingTaken, BeforeHostHealingTaken, this);

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
            var totalHealAbsorb = 0f;
            if (value >= eventData.value)
            {
                value -= eventData.value;
                totalHealAbsorb = eventData.value;
                eventData.SetValue(0f);
            }
            else if (value < eventData.value)
            {
                totalHealAbsorb = value;
                eventData.SetValue(eventData.value - value);
                value = 0f;
                buffInfo.Deplete();
            }

            eventData.SetHealPreventedFromHealAbsorb(totalHealAbsorb);
            hostCombatEvents.InvokeHealthChange(eHealthEvent.OnHealPreventedFromAbsorbShield, eventData);

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
