using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffLethalDamagePrevention : BuffType
    {
        public List<GameObject> buffsToApply;
        [Range(0, 1)]
        public float healthPercent = .25f;

        bool activated;

        Action OnRemoveBeforeHostTakesDamage;
        new void GetDependencies()
        {
            base.GetDependencies();
            if (buffInfo.hostInfo)
            {
                OnRemoveBeforeHostTakesDamage += hostCombatEvents.AddListenerHealth(eHealthEvent.BeforeDamageTaken, BeforeHostTakesDamage, this);
                Debugger.Combat(2895, $"Applied events on {this}");
            }
        }

        private void OnValidate()
        {
            if (buffsToApply == null) return;
            for(int i = 0; i < buffsToApply.Count; i ++)
            {
                var buffObj = buffsToApply[i];
                var buff = buffObj.GetComponent<BuffInfo>();
                
                if (buff == null)
                {
                    buffsToApply.RemoveAt(i);
                    i--;
                }
            }
        }

        public override string Description()
        {

            return FaceDescription(buffInfo.properties.value);
        }

        public override string FaceDescription(float theoreticalValue)
        {
            buffInfo = GetComponent<BuffInfo>();

            var descriptions = new List<string>()
            {
                $"Upon taking lethal damage, negate the amount and set the unit's health to {healthPercent * 100}%."
            };

            if (buffsToApply != null && buffsToApply.Count > 0)
            {
                descriptions.Add("Buffs applied once prevented damage: ");
                foreach (var buff in buffsToApply)
                {
                    var info = buff.GetComponent<BuffInfo>();
                    

                    descriptions.Add($"{info.name}: {info.TypeDescriptionFace(theoreticalValue)}");
                }
            }

            return ArchString.NextLineList(descriptions);
        }

        public override string GeneralDescription()
        {
            var result = "";

            result += $"Upon taking lethal damage, negate the amount and set the unit's health to {healthPercent * 100}% of max health.\n";

            if (buffsToApply != null && buffsToApply.Count > 0)
            {

                result += $"Preventing damage applies: ";
                var buffNames = new List<string>();

                foreach (var buff in buffsToApply)
                {
                    var info = buff.GetComponent<BuffInfo>();
                    buffNames.Add(info.name);
                }

                result += $"{ArchString.StringList(buffNames)}.\n";
            }

            return result;
        }
        void Start()
        {
            GetDependencies();
        }
        void BeforeHostTakesDamage(HealthEvent eventData)
        {
            //if (activated) return;
            if (eventData.target == null) return;
            var healthAndShield = eventData.target.health + eventData.target.shield;
            if (healthAndShield > eventData.value) return;

            Debugger.Combat(2894, $"Prevented {eventData.value} damage");
            //activated = true;
            eventData.SetValue(0f);
            

            eventData.target.health = eventData.target.maxHealth * healthPercent;

            ArchAction.Yield(() => buffInfo.Cleanse());
            OnRemoveBeforeHostTakesDamage?.Invoke();

            var buffsManager = eventData.target.Buffs();

            if (buffsManager == null) return;

            foreach (var buff in buffsToApply)
            {
                buffsManager.ApplyBuff(new(buff, buffInfo));
            }
        }
    }
}
