using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffAOEHealth : BuffType
    {
        [Header("Buff AOE Health Properties")]
        public BuffEvents eventTrigger;
        public AOEType aoeType;
        public BuffTargetType targetType;

        public bool ignoreStructures;



        void Start()
        {
            GetDependencies();
        }

        new void GetDependencies()
        {
            base.GetDependencies();

            var layerMaskData = LayerMasksData.active;


            buffInfo.AddEventAction(eventTrigger, HandleAOE);

        }

        public override string Description()
        {
            var result = buffInfo.buffTargetType == BuffTargetType.Assist ? "Heals allies " : "Damages enemies ";

            var simpleValue = ArchString.FloatToSimple(value);

            result += $"in a {buffInfo.properties.radius} meter radius {ArchString.CamelToTitle(eventTrigger.ToString())} for {simpleValue} health";

            result += eventTrigger == BuffEvents.OnInterval ? $" every {buffInfo.properties.intervals} seconds." : ".";

            return result;
        }

        public override string GeneralDescription()
        {
            var buffInfo = GetComponent<BuffInfo>();
            var result = buffInfo.buffTargetType == BuffTargetType.Assist ? "Heals allies " : "Damages enemies ";

            result += $"in a {buffInfo.properties.radius} meter radius {ArchString.CamelToTitle(eventTrigger.ToString())}";

            result += eventTrigger == BuffEvents.OnInterval ? $" every {buffInfo.properties.intervals} seconds." : ".";

            return result;
        }

        void HandleAOE()
        {
            Predicate<EntityInfo> validEntity = delegate(EntityInfo entity) {
                if (targetType == BuffTargetType.Neutral) return true;
                if (targetType == BuffTargetType.Assist && buffInfo.sourceInfo.CanHelp(entity)) return true;
                if(targetType == BuffTargetType.Harm && buffInfo.sourceInfo.CanAttack(entity)) return true;

                return false;
            };

            var entities = buffInfo.EntitiesWithinRange(!ignoreStructures, validEntity);

            var processedValue = ProcessedValue(entities.Count);

            foreach (var entity in entities)
            {
                buffInfo.HandleTargetHealth(entity, processedValue, targetType);
            }

        }

        public float ProcessedValue(int count)
        {

            return aoeType switch
            {
                AOEType.Distribute => value / count,
                AOEType.Multiply => value * count,
                _ => value,
            };
        }
    }
}
