using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffOnHitEffect : BuffType
    {
        public List<GameObject> onHitBuffs;
        [SerializeField] bool update;

        void Start()
        {
            GetDependencies();
        }

        void Update()
        {

        }

        private void OnValidate()
        {
            if (!update) return; update = false;
            if (onHitBuffs == null) return;

            for (int i = 0; i < onHitBuffs.Count; i++)
            {
                var current = onHitBuffs[i];
                if (current == null)
                {
                    onHitBuffs.RemoveAt(i);
                    i--;
                    continue;
                }

                var info = current.GetComponent<BuffInfo>();

                if (info == null)
                {
                    onHitBuffs.RemoveAt(i);
                    i--;
                }
            }
        }

        new void GetDependencies()
        {
            base.GetDependencies();

            if (buffInfo.hostInfo)
            {
                buffInfo.hostInfo.OnDamageDone += OnHitEffect;
                var unsubScribeHeal = hostCombatEvent.AddListenerHealth(eHealthEvent.OnHealingDone, OnHitEffect, this);


                buffInfo.OnBuffEnd += (BuffInfo) =>
                {
                    buffInfo.hostInfo.OnDamageDone -= OnHitEffect;
                    unsubScribeHeal();
                };
            }
        }
        public override string GeneralDescription()
        {
            var result = "";

            if (onHitBuffs.Count > 0)
            {
                result += "Auto Attacks apply ";
                var stringList = new List<string>();
                foreach (var buff in onHitBuffs)
                {
                    var info = buff.GetComponent<BuffInfo>();

                    stringList.Add(info.name);
                }

                result += $"{ArchString.StringList(stringList)}.";
            }

            return result;
        }

        public override string Description()
        {
            var result = "";

            if (buffInfo.sourceAbility)
            {
                return FaceDescription(buffInfo.sourceAbility.value);
            }

            return result;
        }

        public override string FaceDescription(float theoreticalValue)
        {
            var result = "";

            if (onHitBuffs != null && onHitBuffs.Count > 0)
            {

                var stringList = new List<string>() {
                    "On Hit Effects: "
                };
                
                foreach (var buff in onHitBuffs)
                {
                    var info = buff.GetComponent<BuffInfo>();

                    stringList.Add(info.TypeDescriptionFace(theoreticalValue));
                }

                result += $"{ArchString.NextLineList(stringList)}.";
            }

            return result;
        }


        //void OnHealingDone(CombatEventData eventData)
        //{
        //    if (eventData.ability == null) return;
        //    if (eventData.ability.abilityType2 != AbilityType2.AutoAttack) return;

        //    ApplyOnHit(eventData.target);
        //}
        //void OnDamageDone(CombatEventData eventData)
        //{
        //    if (eventData.ability == null) return;
        //    if (eventData.ability.abilityType2 != AbilityType2.AutoAttack) return;
        //    ApplyOnHit(eventData.target);
        //}

        void OnHitEffect(CombatEventData eventData)
        {
            if (eventData.ability == null) return;
            if (eventData.ability.abilityType2 != AbilityType2.AutoAttack) return;
            ApplyOnHit(eventData.target);
        }

        void OnHitEffect(HealthEvent eventData)
        {
            if (eventData.ability == null) return;
            if (eventData.ability.abilityType2 != AbilityType2.AutoAttack) return;
            ApplyOnHit(eventData.target);
        }

        void ApplyOnHit(EntityInfo target)
        {
            if (target == null) return;

            var buffManager = target.Buffs();

            foreach (var buff in onHitBuffs)
            {
                var info = buff.GetComponent<BuffInfo>();

                if (info.buffTargetType == BuffTargetType.Harm)
                {
                    if (!buffInfo.hostInfo.CanAttack(info.gameObject)) continue;
                }
                if (info.buffTargetType == BuffTargetType.Assist)
                {
                    if (!buffInfo.hostInfo.CanHelp(info.gameObject)) continue;
                }


                buffManager.ApplyBuff(new(buff, buffInfo));
            }
        }

        // Update is called once per frame
        
    }
}
