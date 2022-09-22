using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffHealth : BuffType
    {
        [Header("Buff Health Properties")]
        public BuffEvents eventTrigger;
        public BuffTargetType targetType;

        void Start()
        {
            GetDependencies();
        }

        void Update()
        {
        
        }
        new void GetDependencies()
        {
            base.GetDependencies();

            buffInfo.AddEventAction(eventTrigger, HandleHealth);
        }

        void HandleHealth()
        {
            buffInfo.HandleTargetHealth(buffInfo.hostInfo, value, targetType);
        }

        public override string Description()
        {
            var result = targetType == BuffTargetType.Assist ? "Heals the target " : "Damages the target ";

            var simpleValue = ArchString.FloatToSimple(value);

            result += $"{ArchString.CamelToTitle(eventTrigger.ToString())} for {simpleValue} health";
            result += eventTrigger == BuffEvents.OnInterval ? $" every {buffInfo.properties.intervals} seconds." : ".";

            return result;
        }

        public override string GeneralDescription()
        {
            var buffInfo = GetComponent<BuffInfo>();

            var result = targetType == BuffTargetType.Assist ? "Heals the target " : "Damages the target ";

            result += $"{ArchString.CamelToTitle(eventTrigger.ToString())}";
            result += eventTrigger == BuffEvents.OnInterval ? $" every {buffInfo.properties.intervals} seconds." : ".";

            return result;
        }

        // Update is called once per frame
    }
}
