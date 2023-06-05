using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public static class ShieldExtension
    {
        public static void UpdateShield(this EntityInfo entity)
        {
            var total = 0f;

            var floats = new List<Func<float>>();

            entity.combatEvents.OnUpdateShield?.Invoke(entity, floats);

            foreach(var num in floats)
            {
                total += num();
            }

            entity.shield = total;
        }

        public static void UpdateHealthAbsorbShield(this EntityInfo entity)
        {
            var total = 0f;
            var floats = new List<Func<float>>();

            entity.combatEvents.OnUpdateHealAbsorbShield?.Invoke(entity, floats);

            foreach(var num in floats)
            {
                total += num();
            }

            entity.healAbsorbShield = total;
        }
    }

    public class BuffShield : BuffType
    {
        

        public bool applied;
        new void GetDependencies()
        {
            base.GetDependencies();
            if (buffInfo)
            {


                var entity = buffInfo.hostInfo;

                if (entity)
                {
                    entity.combatEvents.OnUpdateShield += HandleUpdateShield;
                    entity.combatEvents.BeforeDamageTaken += HandleBeforeDamageTaken;
                }

                buffInfo.OnBuffEnd += (BuffInfo buff) => {
                    value = 0f;
                    entity.combatEvents.OnUpdateShield -= HandleUpdateShield;
                    entity.combatEvents.BeforeDamageTaken -= HandleBeforeDamageTaken;
                    entity.UpdateShield();
                };

                ApplyBuff();
            }
        }

        void HandleUpdateShield(EntityInfo entity, List<Func<float>> funcs)
        {
            funcs.Add(() => value);
        }



        public override string Description()
        {
            string result = "";

            result += $"Absorbs the next {ArchString.FloatToSimple(value)} damage.";

            return result;
        }

        public override string GeneralDescription()
        {
            return $"Prevents any type of damage from being taken.";
        }

        void Start()
        {
            GetDependencies();
        }
        void ApplyBuff()
        {
            if (buffInfo && buffInfo.hostInfo)
            {
                applied = true;
                buffInfo.hostInfo.UpdateShield();
            }
        }
        // Update is called once per frame
        void Update()
        {
        }

        void HandleBeforeDamageTaken(CombatEventData eventData)
        {
            if (eventData.value == 0) return;


            if(value >= eventData.value)
            {
                value -= eventData.value;
                eventData.value = 0f;
            }
            else
            {
                eventData.value -= value;
                value = 0f;
                buffInfo.Deplete();
            }

            eventData.target.UpdateShield();
        }
    }

}